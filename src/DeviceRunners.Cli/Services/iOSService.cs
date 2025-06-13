using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DeviceRunners.Cli.Services;

public class iOSService
{
    public void InstallApp(string appPath, string? deviceId = null)
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

        var targetDevice = deviceId ?? GetBootedSimulatorId();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        // Install the app to the simulator
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
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to install app: {error}");
        }
    }

    public void UninstallApp(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app uninstallation is only supported on macOS.");
        }

        var targetDevice = deviceId ?? GetBootedSimulatorId();
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
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to uninstall app: {error}");
        }
    }

    public bool IsAppInstalled(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return false;
        }

        try
        {
            var targetDevice = deviceId ?? GetBootedSimulatorId();
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
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 && output.Contains(bundleIdentifier);
        }
        catch
        {
            return false;
        }
    }

    public void LaunchApp(string bundleIdentifier, string? deviceId = null, string? arguments = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app launching is only supported on macOS.");
        }

        var targetDevice = deviceId ?? GetBootedSimulatorId();
        if (string.IsNullOrEmpty(targetDevice))
        {
            throw new InvalidOperationException("No booted iOS simulator found and no device ID specified.");
        }

        var launchArgs = string.IsNullOrEmpty(arguments) ? "" : $" {arguments}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = $"simctl launch {targetDevice} {bundleIdentifier}{launchArgs}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to launch app: {error}");
        }
    }

    public void TerminateApp(string bundleIdentifier, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS app termination is only supported on macOS.");
        }

        var targetDevice = deviceId ?? GetBootedSimulatorId();
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

    public string GetAppIdentifier(string appPath)
    {
        if (!Directory.Exists(appPath))
        {
            throw new FileNotFoundException($"App bundle not found: {appPath}");
        }

        var plistPath = Path.Combine(appPath, "Info.plist");
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

    public string? GetBootedSimulatorId()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return null;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xcrun",
                Arguments = "simctl list devices --json",
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
            return null;
        }

        try
        {
            var deviceData = JsonSerializer.Deserialize<Dictionary<string, object>>(output);
            if (deviceData?.TryGetValue("devices", out var devicesObj) == true)
            {
                var devices = JsonSerializer.Deserialize<Dictionary<string, JsonElement[]>>(devicesObj.ToString()!);
                if (devices != null)
                {
                    foreach (var runtime in devices)
                    {
                        foreach (var device in runtime.Value)
                        {
                            if (device.TryGetProperty("state", out var state) && 
                                state.GetString() == "Booted" &&
                                device.TryGetProperty("udid", out var udid))
                            {
                                return udid.GetString();
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, fallback to null
        }

        return null;
    }

    public void SaveDeviceLog(string outputPath, string? deviceId = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("iOS device log saving is only supported on macOS.");
        }

        var targetDevice = deviceId ?? GetBootedSimulatorId();
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
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            File.WriteAllText(outputPath, output);
        }
        else
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to get device log: {error}");
        }
    }
}