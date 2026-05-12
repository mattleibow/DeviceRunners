using System.Diagnostics;
using System.Text.Json;

namespace DeviceRunners.Cli.Services;

/// <summary>
/// Wraps the bundled winapp.exe CLI for loose-file MSIX package registration,
/// app launch, and cleanup. Used when the user passes a build output folder
/// or AppxManifest.xml instead of a .msix file.
/// </summary>
public class WinAppService
{
	/// <summary>
	/// Resolves the path to the bundled winapp.exe, selecting the correct
	/// architecture (x64 or arm64) based on the current process.
	/// </summary>
	internal static string GetWinAppPath()
	{
		var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
		{
			System.Runtime.InteropServices.Architecture.Arm64 => "win-arm64",
			_ => "win-x64",
		};

		var cliDir = AppContext.BaseDirectory;
		var winappPath = Path.Combine(cliDir, "winapp", arch, "winapp.exe");

		if (!File.Exists(winappPath))
			throw new FileNotFoundException(
				$"winapp.exe not found at '{winappPath}'. The Microsoft.Windows.SDK.BuildTools.WinApp package may not be installed.",
				winappPath);

		return winappPath;
	}

	/// <summary>
	/// Resolves a user-provided --app value to an (inputFolder, manifestPath) pair.
	/// Accepts:
	///   - A folder containing AppxManifest.xml or Package.appxmanifest
	///   - A direct path to a manifest file
	/// Returns null if the path doesn't look like a loose-file layout.
	/// </summary>
	public static (string inputFolder, string manifestPath)? ResolveLooseLayout(string appPath)
	{
		if (Directory.Exists(appPath))
		{
			var manifest = FindManifestInFolder(appPath);
			if (manifest is not null)
				return (appPath, manifest);
		}
		else if (File.Exists(appPath))
		{
			var fileName = Path.GetFileName(appPath);
			if (fileName.Equals("AppxManifest.xml", StringComparison.OrdinalIgnoreCase)
				|| fileName.Equals("Package.appxmanifest", StringComparison.OrdinalIgnoreCase)
				|| fileName.Equals("appxmanifest.xml", StringComparison.OrdinalIgnoreCase))
			{
				var folder = Path.GetDirectoryName(Path.GetFullPath(appPath))!;
				return (folder, appPath);
			}
		}

		return null;
	}

	static string? FindManifestInFolder(string folder)
	{
		string[] names = ["AppxManifest.xml", "Package.appxmanifest", "appxmanifest.xml"];
		foreach (var name in names)
		{
			var path = Path.Combine(folder, name);
			if (File.Exists(path))
				return path;
		}
		return null;
	}

	/// <summary>
	/// Register and launch the app, returning immediately with the PID.
	/// Uses <c>winapp run --detach --json</c> to get structured output.
	/// </summary>
	public async Task<int> RunDetachedAsync(
		string inputFolder,
		string? manifestPath,
		string? appArgs,
		CancellationToken cancellationToken = default)
	{
		var args = BuildRunArgs(inputFolder, manifestPath, appArgs);
		args += " --detach --json";

		var (exitCode, stdout, stderr) = await RunProcessAsync(GetWinAppPath(), args, cancellationToken);

		if (exitCode != 0)
			throw new InvalidOperationException(
				$"winapp run --detach failed (exit code {exitCode}):\n{stderr}");

		try
		{
			using var doc = JsonDocument.Parse(stdout);
			if (doc.RootElement.TryGetProperty("pid", out var pidElement))
				return pidElement.GetInt32();
		}
		catch (JsonException)
		{
			// Fall through to error
		}

		throw new InvalidOperationException(
			$"Failed to parse PID from winapp output:\n{stdout}");
	}

	/// <summary>
	/// Register and launch the app, blocking until it exits. Returns the app exit code.
	/// </summary>
	public async Task<int> RunAsync(
		string inputFolder,
		string? manifestPath,
		string? appArgs,
		bool unregisterOnExit = false,
		CancellationToken cancellationToken = default)
	{
		var args = BuildRunArgs(inputFolder, manifestPath, appArgs);
		if (unregisterOnExit)
			args += " --unregister-on-exit";

		var (exitCode, _, _) = await RunProcessAsync(GetWinAppPath(), args, cancellationToken);
		return exitCode;
	}

	/// <summary>
	/// Unregister a development package registered via winapp run.
	/// </summary>
	public async Task UnregisterAsync(string manifestPath, CancellationToken cancellationToken = default)
	{
		var args = $"unregister --manifest \"{manifestPath}\"";
		var (exitCode, _, stderr) = await RunProcessAsync(GetWinAppPath(), args, cancellationToken);

		if (exitCode != 0)
			Console.Error.WriteLine($"Warning: winapp unregister returned exit code {exitCode}: {stderr}");
	}

	internal static string BuildRunArgs(string inputFolder, string? manifestPath, string? appArgs)
	{
		var args = $"run \"{inputFolder}\"";
		if (manifestPath is not null)
			args += $" --manifest \"{manifestPath}\"";
		if (!string.IsNullOrEmpty(appArgs))
			args += $" --args \"{appArgs.Replace("\"", "\\\"")}\"";
		return args;
	}

	static async Task<(int exitCode, string stdout, string stderr)> RunProcessAsync(
		string fileName, string arguments, CancellationToken cancellationToken)
	{
		var psi = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
		};

		using var process = Process.Start(psi)
			?? throw new InvalidOperationException($"Failed to start: {fileName}");

		var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
		var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

		await process.WaitForExitAsync(cancellationToken);

		return (process.ExitCode, await stdoutTask, await stderrTask);
	}
}
