using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class iOSTestCommand(IAnsiConsole console) : BaseTestCommand<iOSTestCommand.Settings>(console)
{
    public class Settings : BaseTestCommandSettings
    {
        [Description("iOS Simulator device ID (optional, will use booted simulator if not specified)")]
        [CommandOption("--device")]
        public string? Device { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var testStartTime = DateTimeOffset.Now;
        
        try
        {
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]PREPARATION[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            var iOSService = new iOSService();

            // Get app identifier
            WriteConsoleOutput($"  - Determining app identifier...", settings);
            var appIdentifier = iOSService.GetAppIdentifier(settings.App);
            WriteConsoleOutput($"    App bundle: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            WriteConsoleOutput($"    App identifier found: '[green]{Markup.Escape(appIdentifier)}[/]'", settings);

            // Check for booted simulator or use specified device
            string? targetDevice = null;
            if (!string.IsNullOrEmpty(settings.Device))
            {
                targetDevice = settings.Device;
                WriteConsoleOutput($"  - Using specified device: [green]{Markup.Escape(targetDevice)}[/]", settings);
            }
            else
            {
                targetDevice = await iOSService.GetBootedSimulatorIdAsync();
                if (string.IsNullOrEmpty(targetDevice))
                {
                    var errorResult = new TestStartResult
                    {
                        Success = false,
                        ErrorMessage = "No booted iOS simulator found. Please boot a simulator or specify a device ID with --device."
                    };

                    WriteConsoleOutput($"[red]Error: No booted iOS simulator found. Please boot a simulator or specify a device ID with --device.[/]", settings);
                    WriteResult(errorResult, settings);
                    return 1;
                }
                WriteConsoleOutput($"  - Using booted simulator: [green]{Markup.Escape(targetDevice)}[/]", settings);
                settings.Device = targetDevice;
            }

            var simDetails = await iOSService.GetSimulatorDetailsAsync(settings.Device);
            if (simDetails is not null)
            {
                if (!string.IsNullOrEmpty(simDetails.Name))
                    WriteConsoleOutput($"    Name: '[green]{Markup.Escape(simDetails.Name)}[/]'", settings);
                if (!string.IsNullOrEmpty(simDetails.Runtime?.Version))
                    WriteConsoleOutput($"    OS Version: '[green]{Markup.Escape(simDetails.Runtime.Version)}[/]'", settings);
            }

            // Check if app is already installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (await iOSService.IsAppInstalledAsync(appIdentifier, targetDevice))
            {
                WriteConsoleOutput($"    App was installed, uninstalling...", settings);
                await iOSService.UninstallAppAsync(appIdentifier, targetDevice);
                WriteConsoleOutput($"    Uninstall complete...", settings);
            }
            else
            {
                WriteConsoleOutput($"    App was not installed.", settings);
            }

            // Install the app
            WriteConsoleOutput($"  - Installing the app...", settings);
            await iOSService.InstallAppAsync(settings.App, targetDevice);
            WriteConsoleOutput($"    Application installed.", settings);

            // Start the app
            WriteConsoleOutput($"  - Starting the application...", settings);
            var envVars = new Dictionary<string, string>
            {
                ["DEVICE_RUNNERS_AUTORUN"] = "1",
                ["DEVICE_RUNNERS_PORT"] = settings.Port.ToString(),
                ["DEVICE_RUNNERS_HOST_NAMES"] = "localhost",
            };
            await iOSService.LaunchAppAsync(appIdentifier, targetDevice, envVars);
            WriteConsoleOutput($"    Application started.", settings);

            // Handle TCP test results
            var listener = await StartTestListener(settings);

            WriteConsoleOutput($"", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            // Terminate the app
            WriteConsoleOutput($"  - Terminating the application...", settings);
            await iOSService.TerminateAppAsync(appIdentifier, targetDevice);
            WriteConsoleOutput($"    Application terminated.", settings);

            // Save device log (filtered to app process for managed exception details)
            var appName = Path.GetFileNameWithoutExtension(settings.App);
            var deviceLogFile = GetDeviceLogFilePath(settings);
            WriteConsoleOutput($"  - Saving device log to: [green]{Markup.Escape(deviceLogFile)}[/]", settings);
            try
            {
                await iOSService.SaveDeviceLogAsync(deviceLogFile, testStartTime, targetDevice, processName: appName);
                WriteConsoleOutput($"    Device log saved.", settings);
            }
            catch (Exception logEx)
            {
                WriteConsoleOutput($"[yellow]Warning: Failed to save device log: {Markup.Escape(logEx.Message)}[/]", settings);
            }

            WriteConsoleOutput($"  - Cleanup complete.", settings);

            var result = new TestStartResult
            {
                Success = listener.Success,
                AppIdentity = appIdentifier,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory,
                TestFailures = listener.FailedCount,
                TestResults = listener.ResultsFile,
                DeviceLogFile = deviceLogFile
            };
            WriteResult(result, settings);

            // Exit codes: 0 = success, 1 = test failures, 2 = app crashed
            return listener.ToExitCode();
        }
        catch (Exception ex)
        {
            var iOSService = new iOSService();
            var deviceLogFile = GetDeviceLogFilePath(settings);
            
            try
            {
                WriteConsoleOutput($"  - Saving device log due to error: [green]{Markup.Escape(deviceLogFile)}[/]", settings);
                await iOSService.SaveDeviceLogAsync(deviceLogFile, testStartTime, settings.Device, processName: Path.GetFileNameWithoutExtension(settings.App));
                WriteConsoleOutput($"    Device log saved.", settings);
            }
            catch (Exception logEx)
            {
                WriteConsoleOutput($"[yellow]Warning: Failed to save device log: {Markup.Escape(logEx.Message)}[/]", settings);
            }

            var errorResult = new TestStartResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory,
                DeviceLogFile = deviceLogFile
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(errorResult, settings);
            return 1;
        }
    }

    private string GetDeviceLogFilePath(Settings settings)
    {
        var resultsDir = settings.ResultsDirectory ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(resultsDir);
        return Path.Combine(resultsDir, "ios-device-log.txt");
    }
}