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
                targetDevice = iOSService.GetBootedSimulatorId();
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
            }

            // Check if app is already installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (iOSService.IsAppInstalled(appIdentifier, targetDevice))
            {
                WriteConsoleOutput($"    App was installed, uninstalling...", settings);
                iOSService.UninstallApp(appIdentifier, targetDevice);
                WriteConsoleOutput($"    Uninstall complete...", settings);
            }
            else
            {
                WriteConsoleOutput($"    App was not installed.", settings);
            }

            // Install the app
            WriteConsoleOutput($"  - Installing the app...", settings);
            iOSService.InstallApp(settings.App, targetDevice);
            WriteConsoleOutput($"    Application installed.", settings);

            // Start the app
            WriteConsoleOutput($"  - Starting the application...", settings);
            iOSService.LaunchApp(appIdentifier, targetDevice);
            WriteConsoleOutput($"    Application started.", settings);

            // Handle TCP test results
            var (testFailures, testResults) = await StartTestListener(settings);

            WriteConsoleOutput($"", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            // Terminate the app
            WriteConsoleOutput($"  - Terminating the application...", settings);
            iOSService.TerminateApp(appIdentifier, targetDevice);
            WriteConsoleOutput($"    Application terminated.", settings);

            // Save device log
            var deviceLogFile = GetDeviceLogFilePath(settings);
            WriteConsoleOutput($"  - Saving device log to: [green]{Markup.Escape(deviceLogFile)}[/]", settings);
            try
            {
                iOSService.SaveDeviceLog(deviceLogFile, targetDevice);
                WriteConsoleOutput($"    Device log saved.", settings);
            }
            catch (Exception logEx)
            {
                WriteConsoleOutput($"[yellow]Warning: Failed to save device log: {Markup.Escape(logEx.Message)}[/]", settings);
            }

            WriteConsoleOutput($"  - Cleanup complete.", settings);

            var result = new TestStartResult
            {
                Success = testFailures == 0,
                AppIdentity = appIdentifier,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory,
                TestFailures = testFailures,
                TestResults = testResults,
                DeviceLogFile = deviceLogFile
            };
            WriteResult(result, settings);

            return testFailures > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            var iOSService = new iOSService();
            var deviceLogFile = GetDeviceLogFilePath(settings);
            
            try
            {
                WriteConsoleOutput($"  - Saving device log due to error: [green]{Markup.Escape(deviceLogFile)}[/]", settings);
                iOSService.SaveDeviceLog(deviceLogFile, settings.Device);
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