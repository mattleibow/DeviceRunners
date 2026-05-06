using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DeviceRunners.Cli.Services;

public class MacOSService
{
    public void InstallApp(string appPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("macOS app installation is only supported on macOS.");
        }

        if (!Directory.Exists(appPath))
        {
            throw new FileNotFoundException($"App bundle not found: {appPath}");
        }

        if (!appPath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified path is not a .app bundle.", nameof(appPath));
        }

        // Copy the app to Applications folder (requires sudo for global install)
        // For testing purposes, we'll use a local install approach
        var appName = Path.GetFileName(appPath);
        var userApplicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications");
        
        if (!Directory.Exists(userApplicationsPath))
        {
            Directory.CreateDirectory(userApplicationsPath);
        }

        var targetPath = Path.Combine(userApplicationsPath, appName);
        
        // Remove existing installation if it exists
        if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }

        // Copy the app bundle
        CopyDirectory(appPath, targetPath);
    }

    public void UninstallApp(string appPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("macOS app uninstallation is only supported on macOS.");
        }

        var appName = Path.GetFileName(appPath);
        var userApplicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications");
        var targetPath = Path.Combine(userApplicationsPath, appName);

        if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }
    }

    public bool IsAppInstalled(string appPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return false;
        }

        var appName = Path.GetFileName(appPath);
        var userApplicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications");
        var targetPath = Path.Combine(userApplicationsPath, appName);

        return Directory.Exists(targetPath);
    }

    public void LaunchApp(string appPath, string? arguments = null, IDictionary<string, string>? environmentVariables = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("macOS app launching is only supported on macOS.");
        }

        var appName = Path.GetFileName(appPath);
        var userApplicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications");
        var targetPath = Path.Combine(userApplicationsPath, appName);

        if (!Directory.Exists(targetPath))
        {
            throw new DirectoryNotFoundException($"App is not installed: {appName}");
        }

        // Launch the executable inside the bundle directly so we retain the process
        // handle (useful for future crash detection) and avoid LaunchServices overhead.
        // Note: 'open --env KEY=VALUE' also supports env var injection, but direct
        // launch is simpler and gives us more control for headless test scenarios.
        var executableName = GetBundleExecutableName(targetPath);
        var executablePath = Path.Combine(targetPath, "Contents", "MacOS", executableName);

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"Executable not found inside app bundle: {executablePath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(executablePath)
        };

        if (environmentVariables is not null)
        {
            foreach (var (key, value) in environmentVariables)
                startInfo.EnvironmentVariables[key] = value;
        }

        if (!string.IsNullOrEmpty(arguments))
        {
            startInfo.Arguments = arguments;
        }

        var process = new Process { StartInfo = startInfo };
        process.Start();
    }

    public string GetAppIdentifier(string appPath)
    {
        return GetPlistValue(appPath, "CFBundleIdentifier")
            ?? throw new InvalidOperationException("CFBundleIdentifier not found in Info.plist");
    }

    public string GetBundleExecutableName(string appPath)
    {
        // Prefer the explicit bundle executable name from Info.plist; fall back to
        // stripping the .app extension if the plist isn't available.
        return GetPlistValue(appPath, "CFBundleExecutable")
            ?? Path.GetFileNameWithoutExtension(Path.GetFileName(appPath));
    }

    private string? GetPlistValue(string appPath, string key)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return null;

        var plistPath = Path.Combine(appPath, "Contents", "Info.plist");
        if (!File.Exists(plistPath))
            throw new FileNotFoundException($"Info.plist not found in app bundle: {plistPath}");

        // Use plutil to convert plist to JSON so we can read it without a native plist library.
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "plutil",
                Arguments = $"-convert json -o - \"{plistPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to read Info.plist: {error}");
        }

        try
        {
            var plistData = JsonSerializer.Deserialize<Dictionary<string, object>>(output);
            if (plistData != null && plistData.TryGetValue(key, out var value))
                return value.ToString();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse Info.plist as JSON: {ex.Message}");
        }

        return null;
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        // Copy subdirectories
        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
            CopyDirectory(directory, destSubDir);
        }
    }
}