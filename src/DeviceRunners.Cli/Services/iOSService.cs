using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DeviceRunners.Cli.Services;

public class iOSService
{
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

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = $"simctl install {targetDevice} \"{appPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to install app: {error}");
        }
    }

    public void InstallApp(string appPath, string? deviceId = null)
    {
        InstallAppAsync(appPath, deviceId).GetAwaiter().GetResult();
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

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = $"simctl uninstall {targetDevice} {bundleIdentifier}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to uninstall app: {error}");
        }
    }

    public void UninstallApp(string bundleIdentifier, string? deviceId = null)
    {
        UninstallAppAsync(bundleIdentifier, deviceId).GetAwaiter().GetResult();
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

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xcrun",
                    Arguments = $"simctl listapps {targetDevice}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return process.ExitCode == 0 && output.Contains(bundleIdentifier);
        }
        catch
        {
            return false;
        }
    }

    public bool IsAppInstalled(string bundleIdentifier, string? deviceId = null)
    {
        return IsAppInstalledAsync(bundleIdentifier, deviceId).GetAwaiter().GetResult();
    }

    public async Task LaunchAppAsync(string bundleIdentifier, string? deviceId = null, string? arguments = null)
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

        var args = string.IsNullOrEmpty(arguments) 
            ? $"simctl launch {targetDevice} {bundleIdentifier}"
            : $"simctl launch {targetDevice} {bundleIdentifier} {arguments}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to launch app: {error}");
        }
    }

    public void LaunchApp(string bundleIdentifier, string? deviceId = null, string? arguments = null)
    {
        LaunchAppAsync(bundleIdentifier, deviceId, arguments).GetAwaiter().GetResult();
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

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = $"simctl terminate {targetDevice} {bundleIdentifier}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        process.WaitForExit();

        // Note: terminating an app that's not running returns exit code 1, which is expected
    }

    public void TerminateApp(string bundleIdentifier, string? deviceId = null)
    {
        TerminateAppAsync(bundleIdentifier, deviceId).GetAwaiter().GetResult();
    }

    public string GetAppIdentifier(string appPath)
    {
        if (!Directory.Exists(appPath))
        {
            throw new FileNotFoundException($"App bundle not found: {appPath}");
        }

        var infoPlistPath = Path.Combine(appPath, "Info.plist");
        if (!File.Exists(infoPlistPath))
        {
            throw new FileNotFoundException($"Info.plist not found in app bundle: {infoPlistPath}");
        }

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "plutil",
                    Arguments = $"-convert json -o - \"{infoPlistPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to read Info.plist: {error}");
            }

            var plistData = JsonSerializer.Deserialize<Dictionary<string, object>>(output);
            if (plistData != null && plistData.TryGetValue("CFBundleIdentifier", out var bundleId))
            {
                var bundleIdentifier = bundleId.ToString();
                if (string.IsNullOrEmpty(bundleIdentifier))
                {
                    throw new InvalidOperationException("CFBundleIdentifier is empty in Info.plist");
                }
                return bundleIdentifier;
            }

            throw new InvalidOperationException("CFBundleIdentifier not found in Info.plist");
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
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "appledev simulator list --booted --format json",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return null;
            }

            var simulators = JsonSerializer.Deserialize<JsonElement[]>(output);
            if (simulators != null && simulators.Length > 0)
            {
                var firstSim = simulators[0];
                if (firstSim.TryGetProperty("UDID", out var udidProperty))
                {
                    return udidProperty.GetString();
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public string? GetBootedSimulatorId()
    {
        return GetBootedSimulatorIdAsync().GetAwaiter().GetResult();
    }

    public async Task SaveDeviceLogAsync(string outputPath, string? deviceId = null)
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

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = $"simctl spawn {targetDevice} log show --style syslog --start '1970-01-01 00:00:00'",
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

    public void SaveDeviceLog(string outputPath, string? deviceId = null)
    {
        SaveDeviceLogAsync(outputPath, deviceId).GetAwaiter().GetResult();
    }
}