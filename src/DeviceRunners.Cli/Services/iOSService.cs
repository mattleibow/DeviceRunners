using System.Diagnostics;
using System.Runtime.InteropServices;

using AppleDev;

namespace DeviceRunners.Cli.Services;

public class iOSService
{
    private readonly SimCtl _simCtl;

    public iOSService()
    {
        _simCtl = new SimCtl();
    }

    public async Task InstallAppAsync(string appPath, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app installation is only supported on macOS.");
        }

        if (!Directory.Exists(appPath))
        {
            throw new FileNotFoundException($"App bundle not found: {appPath}");
        }

        if (!appPath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified path is not a .app bundle.", nameof(appPath));
        }

        var targetDevice = deviceId ?? await GetBootedSimulatorIdAsync();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        var appDirectory = new DirectoryInfo(appPath);
        var success = await _simCtl.InstallAppAsync(targetDevice, appDirectory);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to install app: {appPath}");
        }
    }

    public async Task UninstallAppAsync(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app uninstallation is only supported on macOS.");
        }

        var targetDevice = deviceId ?? await GetBootedSimulatorIdAsync();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        var success = await _simCtl.UninstallAppAsync(targetDevice, bundleIdentifier);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to uninstall app: {bundleIdentifier}");
        }
    }

    public async Task<bool> IsAppInstalledAsync(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return false;
        }

        try
        {
            var targetDevice = deviceId ?? await GetBootedSimulatorIdAsync();
            if (string.IsNullOrEmpty(targetDevice))
            {
                return false;
            }

            // Use AppleDev.SimCtl to get installed apps
            var apps = await _simCtl.GetAppsAsync(targetDevice);
            return apps.Any(app => app.CFBundleIdentifier == bundleIdentifier);
        }
        catch
        {
            return false;
        }
    }

    public async Task LaunchAppAsync(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app launching is only supported on macOS.");
        }

        var targetDevice = deviceId ?? await GetBootedSimulatorIdAsync();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        var success = await _simCtl.LaunchAppAsync(targetDevice, bundleIdentifier);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to launch app: {bundleIdentifier}");
        }
    }

    public async Task TerminateAppAsync(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app termination is only supported on macOS.");
        }

        var targetDevice = deviceId ?? await GetBootedSimulatorIdAsync();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        // Use AppleDev.SimCtl for app termination
        var success = await _simCtl.TerminateAppAsync(targetDevice, bundleIdentifier);

        // Note: terminating an app that's not running returns false, which is expected
    }

    public string GetAppIdentifier(string appPath)
    {
        if (!Directory.Exists(appPath))
        {
            throw new FileNotFoundException($"App bundle not found: {appPath}");
        }

        try
        {
            var bundleReader = new AppBundleReader(appPath);
            var infoPlist = bundleReader.ReadInfoPlist();

            var bundleIdentifier = infoPlist.CFBundleIdentifier;
            if (string.IsNullOrEmpty(bundleIdentifier))
            {
                throw new InvalidOperationException("CFBundleIdentifier not found in Info.plist");
            }

            return bundleIdentifier;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to read app bundle info: {ex.Message}", ex);
        }
    }

    public async Task<string?> GetBootedSimulatorIdAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return null;
        }

        try
        {
            var simulators = await _simCtl.GetSimulatorsAsync(availableOnly: true);
            var bootedSimulator = simulators.FirstOrDefault(s => s.IsBooted);
            return bootedSimulator?.Udid;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveDeviceLogAsync(string outputPath, DateTime? startDate = null, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS device log saving is only supported on macOS.");
        }

        var targetDevice = deviceId ?? await GetBootedSimulatorIdAsync();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        var startDateString = startDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "1970-01-01 00:00:00";
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = $"simctl spawn {targetDevice} log show --style syslog --start '{startDateString}'",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            await File.WriteAllTextAsync(outputPath, output);
        }
        else
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Failed to get device log: {error}");
        }
    }
}
