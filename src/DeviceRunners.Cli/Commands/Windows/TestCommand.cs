using System.ComponentModel;
using System.Diagnostics;

using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WindowsTestCommand(IAnsiConsole console) : BaseTestCommand<WindowsTestCommand.Settings>(console)
{
	public class Settings : BaseTestCommandSettings
	{
		[Description("Path to the certificate file")]
		[CommandOption("--certificate")]
		public string? Certificate { get; set; }
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		try
		{
			WriteConsoleOutput($"[blue]============================================================[/]", settings);
			WriteConsoleOutput($"[blue]PREPARATION[/]", settings);
			WriteConsoleOutput($"[blue]============================================================[/]", settings);

			// Check if this is an unpackaged app (.exe), loose MSIX layout (folder/manifest), or packaged app (.msix)
			if (string.IsNullOrEmpty(settings.App))
			{
				throw new ArgumentException("Application path is required. Use --app to specify the path to your application.");
			}

			var looseLayout = WinAppService.ResolveLooseLayout(settings.App);
			if (looseLayout.HasValue)
			{
				return await ExecuteLoosePackagedApp(settings, looseLayout.Value.inputFolder, looseLayout.Value.manifestPath);
			}

			var extension = Path.GetExtension(settings.App).ToLowerInvariant();
			if (extension == ".exe")
			{
				return await ExecuteUnpackagedApp(settings);
			}
			else
			{
				return await ExecutePackagedApp(settings);
			}
		}
		catch (Exception ex)
		{
			var result = new TestStartResult
			{
				Success = false,
				ErrorMessage = ex.Message,
				AppPath = settings.App,
				ResultsDirectory = settings.ResultsDirectory
			};

			WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
			WriteResult(result, settings);
			return 1;
		}
	}

	private async Task<int> ExecuteUnpackagedApp(Settings settings)
	{
		WriteConsoleOutput($"  - Validating unpackaged application...", settings);

		// Validate that the app file exists and is an executable
		if (!File.Exists(settings.App))
		{
			throw new FileNotFoundException($"Application file not found: {settings.App}");
		}

		WriteConsoleOutput($"    Application file: '[green]{Markup.Escape(settings.App)}[/]'", settings);
		WriteConsoleOutput($"    Application validated.", settings);

		WriteConsoleOutput($"", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);
		WriteConsoleOutput($"[blue]EXECUTION[/]", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);

		// Start the unpackaged application directly, injecting env vars so the app
		// auto-configures headless mode (no compile-time flags needed).
		WriteConsoleOutput($"  - Starting the unpackaged application...", settings);

		var startInfo = new ProcessStartInfo
		{
			FileName = settings.App,
			UseShellExecute = false,
			WorkingDirectory = Path.GetDirectoryName(settings.App)
		};

		foreach (var kvp in GetAppEnvironmentVariables(settings))
			startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;

		var process = Process.Start(startInfo);
		if (process == null)
		{
			throw new InvalidOperationException("Failed to start the application process.");
		}

		WriteConsoleOutput($"    Application started with PID: {process.Id}", settings);

		// Handle TCP test results
		var listener = await StartTestListener(settings);

		WriteConsoleOutput($"", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);
		WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);

		// For unpackaged apps, we just try to terminate the process if it's still running
		WriteConsoleOutput($"  - Checking application process...", settings);
		try
		{
			if (!process.HasExited)
			{
				WriteConsoleOutput($"    Application is still running, terminating...", settings);
				process.Kill();
				process.WaitForExit(5000); // Wait up to 5 seconds for graceful exit
				WriteConsoleOutput($"    Application terminated.", settings);
			}
			else
			{
				WriteConsoleOutput($"    Application has already exited.", settings);
			}
		}
		catch (Exception ex)
		{
			WriteConsoleOutput($"    [yellow]Warning: Failed to check/terminate application process: {Markup.Escape(ex.Message)}[/]", settings);
		}

		WriteConsoleOutput($"  - Cleanup complete.", settings);

		var result = new TestStartResult
		{
			Success = listener.Success,
			AppPath = settings.App,
			ResultsDirectory = settings.ResultsDirectory,
			TestFailures = listener.FailedCount,
			TestResults = listener.ResultsFile
		};
		WriteResult(result, settings);

		// Exit codes: 0 = success, 1 = test failures, 2 = app crashed
		return listener.ToExitCode();
	}

	private async Task<int> ExecuteLoosePackagedApp(Settings settings, string inputFolder, string manifestPath)
	{
		var winAppService = new WinAppService();

		WriteConsoleOutput($"  - Loose MSIX layout detected.", settings);
		WriteConsoleOutput($"    Input folder: '[green]{Markup.Escape(inputFolder)}[/]'", settings);
		WriteConsoleOutput($"    Manifest:     '[green]{Markup.Escape(manifestPath)}[/]'", settings);

		WriteConsoleOutput($"", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);
		WriteConsoleOutput($"[blue]EXECUTION[/]", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);

		// Register and launch the app via winapp.exe, returning immediately with PID
		WriteConsoleOutput($"  - Registering and launching via winapp.exe (--detach)...", settings);
		int pid;
		try
		{
			var appPort = settings.AppPort ?? settings.Port;
			var appHostNames = settings.AppHostNames ?? "localhost";
			var appArgs = $"--device-runners-autorun --device-runners-port {appPort} --device-runners-host-names {appHostNames}";
			pid = await winAppService.RunDetachedAsync(inputFolder, manifestPath, appArgs: appArgs);
		}
		catch (Exception ex)
		{
			WriteConsoleOutput($"    [red]Failed to launch: {Markup.Escape(ex.Message)}[/]", settings);
			throw;
		}
		WriteConsoleOutput($"    Application launched with PID: {pid}", settings);

		// Handle TCP test results
		var listener = await StartTestListener(settings);

		WriteConsoleOutput($"", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);
		WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);

		// Terminate the process if still running
		WriteConsoleOutput($"  - Checking application process...", settings);
		try
		{
			var process = Process.GetProcessById(pid);
			if (!process.HasExited)
			{
				WriteConsoleOutput($"    Application is still running, terminating...", settings);
				process.Kill(entireProcessTree: true);
				process.WaitForExit(5000);
				WriteConsoleOutput($"    Application terminated.", settings);
			}
			else
			{
				WriteConsoleOutput($"    Application has already exited.", settings);
			}
		}
		catch (ArgumentException)
		{
			WriteConsoleOutput($"    Application process has already exited.", settings);
		}
		catch (Exception ex)
		{
			WriteConsoleOutput($"    [yellow]Warning: Failed to check/terminate process: {Markup.Escape(ex.Message)}[/]", settings);
		}

		// Unregister the development package
		WriteConsoleOutput($"  - Unregistering development package...", settings);
		try
		{
			await winAppService.UnregisterAsync(manifestPath);
			WriteConsoleOutput($"    Package unregistered.", settings);
		}
		catch (Exception ex)
		{
			WriteConsoleOutput($"    [yellow]Warning: Failed to unregister: {Markup.Escape(ex.Message)}[/]", settings);
		}

		WriteConsoleOutput($"  - Cleanup complete.", settings);

		var result = new TestStartResult
		{
			Success = listener.Success,
			AppPath = settings.App,
			ResultsDirectory = settings.ResultsDirectory,
			TestFailures = listener.FailedCount,
			TestResults = listener.ResultsFile
		};
		WriteResult(result, settings);

		return listener.ToExitCode();
	}

	private async Task<int> ExecutePackagedApp(Settings settings)
	{
		var packageService = new PackageService();
		var certificateService = new CertificateService();

		// Determine certificate
		var certificatePath = settings.Certificate ?? packageService.GetCertificateFromMsix(settings.App);
		var certFingerprint = certificateService.GetCertificateFingerprint(certificatePath);

		WriteConsoleOutput($"  - Determining certificate for MSIX installer...", settings);
		WriteConsoleOutput($"    File path: '[green]{Markup.Escape(certificatePath)}[/]'", settings);
		WriteConsoleOutput($"    Thumbprint: '[green]{certFingerprint}[/]'", settings);
		WriteConsoleOutput($"    Certificate identified.", settings);

		// Determine app identity
		WriteConsoleOutput($"  - Determining app identity...", settings);
		var appIdentity = packageService.GetPackageIdentity(settings.App);
		WriteConsoleOutput($"    MSIX installer: '[green]{Markup.Escape(settings.App)}[/]'", settings);
		WriteConsoleOutput($"    App identity found: '[green]{Markup.Escape(appIdentity)}[/]'", settings);

		// Check if app is already installed
		WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
		if (packageService.IsPackageInstalled(appIdentity))
		{
			WriteConsoleOutput($"    App was installed, uninstalling...", settings);
			packageService.UninstallPackage(appIdentity);
			WriteConsoleOutput($"    Uninstall complete...", settings);
		}
		else
		{
			WriteConsoleOutput($"    App was not installed.", settings);
		}

		// Check certificate installation
		var autoInstalledCertificate = false;
		WriteConsoleOutput($"  - Testing available certificates...", settings);
		if (!certificateService.IsCertificateInstalled(certFingerprint))
		{
			autoInstalledCertificate = true;
			WriteConsoleOutput($"    Certificate was not found, importing certificate...", settings);
			certificateService.InstallCertificate(certificatePath);
			WriteConsoleOutput($"    Certificate imported.", settings);
		}
		else
		{
			WriteConsoleOutput($"    Certificate was found.", settings);
		}

		// Install dependencies first
		WriteConsoleOutput($"  - Installing dependencies...", settings);
		var dependencies = packageService.GetDependencies(settings.App);
		foreach (var dependency in dependencies)
		{
			try
			{
				WriteConsoleOutput($"    Installing dependency: '[green]{Markup.Escape(dependency)}[/]'", settings);
				packageService.InstallPackage(dependency);
			}
			catch
			{
				WriteConsoleOutput($"    Dependency failed to install, continuing...", settings);
			}
		}

		// Install the app
		WriteConsoleOutput($"  - Installing the app...", settings);
		packageService.InstallPackage(settings.App);
		WriteConsoleOutput($"    Application installed.", settings);

		// Start the app
		WriteConsoleOutput($"  - Starting the application...", settings);
		packageService.LaunchApp(appIdentity, null);
		WriteConsoleOutput($"    Application started.", settings);

		// Handle TCP test results
		var listener = await StartTestListener(settings);

		WriteConsoleOutput($"", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);
		WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
		WriteConsoleOutput($"[blue]============================================================[/]", settings);

		// Cleanup: Uninstall the app
		WriteConsoleOutput($"  - Uninstalling application...", settings);
		try
		{
			packageService.UninstallPackage(appIdentity);
			WriteConsoleOutput($"    Application uninstalled.", settings);
		}
		catch (Exception ex)
		{
			WriteConsoleOutput($"    [yellow]Warning: Failed to uninstall application: {Markup.Escape(ex.Message)}[/]", settings);
		}

		// Cleanup: Remove certificate if we auto-installed it
		if (autoInstalledCertificate)
		{
			WriteConsoleOutput($"  - Removing installed certificates...", settings);
			try
			{
				certificateService.UninstallCertificate(certFingerprint);
				WriteConsoleOutput($"    Installed certificates removed.", settings);
			}
			catch (Exception ex)
			{
				WriteConsoleOutput($"    [yellow]Warning: Failed to remove certificate: {Markup.Escape(ex.Message)}[/]", settings);
			}
		}

		WriteConsoleOutput($"  - Cleanup complete.", settings);

		var result = new TestStartResult
		{
			Success = listener.Success,
			AppIdentity = appIdentity,
			AppPath = settings.App,
			CertificateThumbprint = certFingerprint,
			ResultsDirectory = settings.ResultsDirectory,
			TestFailures = listener.FailedCount,
			TestResults = listener.ResultsFile
		};
		WriteResult(result, settings);

		// Exit codes: 0 = success, 1 = test failures, 2 = app crashed
		return listener.ToExitCode();
	}
}

