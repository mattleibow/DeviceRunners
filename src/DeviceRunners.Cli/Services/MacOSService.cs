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

    public void LaunchApp(string appPath, string? arguments = null)
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

        // Use 'open' command to launch the app
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{targetPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        if (!string.IsNullOrEmpty(arguments))
        {
            process.StartInfo.Arguments += $" --args {arguments}";
        }

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to launch app: {error}");
        }
    }

    public string GetAppIdentifier(string appPath)
    {
        if (!Directory.Exists(appPath))
        {
            throw new FileNotFoundException($"App bundle not found: {appPath}");
        }

        var plistPath = Path.Combine(appPath, "Contents", "Info.plist");
        if (!File.Exists(plistPath))
        {
            throw new FileNotFoundException($"Info.plist not found in app bundle: {plistPath}");
        }

        // Use plutil to convert plist to JSON and extract bundle identifier
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
            if (plistData != null && plistData.TryGetValue("CFBundleIdentifier", out var identifier))
            {
                return identifier.ToString()!;
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse Info.plist as JSON: {ex.Message}");
        }

        throw new InvalidOperationException("CFBundleIdentifier not found in Info.plist");
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