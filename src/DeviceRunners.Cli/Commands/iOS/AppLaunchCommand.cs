using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class iOSAppLaunchCommand(IAnsiConsole console) : BaseAsyncCommand<iOSAppLaunchCommand.Settings>(console)
{
    public class Settings : BaseAsyncCommandSettings
    {
        [Description("Path to the .app application bundle (to determine bundle identifier)")]
        [CommandOption("--app")]
        public string? App { get; set; }
        
        [Description("Bundle identifier to launch")]
        [CommandOption("--bundle-id")]
        public string? BundleId { get; set; }
        
        [Description("iOS Simulator device ID (optional, will use booted simulator if not specified)")]
        [CommandOption("--device")]
        public string? Device { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var iOSService = new iOSService();
            string bundleIdentifier;

            // Display device info if specified
            if (!string.IsNullOrEmpty(settings.Device))
            {
                WriteConsoleOutput($"  - Using device: [green]{Markup.Escape(settings.Device)}[/]", settings);
            }
            else
            {
                var bootedDevice = await iOSService.GetBootedSimulatorIdAsync();
                if (string.IsNullOrEmpty(bootedDevice))
                {
                    var result = new AppLaunchResult
                    {
                        Success = false,
                        ErrorMessage = "No booted iOS simulator found. Please boot a simulator or specify a device ID with --device."
                    };

                    WriteConsoleOutput($"[red]Error: No booted iOS simulator found. Please boot a simulator or specify a device ID with --device.[/]", settings);
                    WriteResult(result, settings);
                    return 1;
                }
                WriteConsoleOutput($"  - Using booted simulator: [green]{Markup.Escape(bootedDevice)}[/]", settings);
                settings.Device = bootedDevice;
            }

            var simDetails = await iOSService.GetSimulatorDetailsAsync(settings.Device);
            if (simDetails is not null)
            {
                if (!string.IsNullOrEmpty(simDetails.Name))
                    WriteConsoleOutput($"    Name: '[green]{Markup.Escape(simDetails.Name)}[/]'", settings);
                if (!string.IsNullOrEmpty(simDetails.Runtime?.Version))
                    WriteConsoleOutput($"    OS Version: '[green]{Markup.Escape(simDetails.Runtime.Version)}[/]'", settings);
            }

            if (!string.IsNullOrEmpty(settings.BundleId))
            {
                bundleIdentifier = settings.BundleId;
                WriteConsoleOutput($"  - Using provided bundle identifier: '[green]{Markup.Escape(bundleIdentifier)}[/]'", settings);
            }
            else if (!string.IsNullOrEmpty(settings.App))
            {
                WriteConsoleOutput($"  - Determining bundle identifier from app...", settings);
                bundleIdentifier = iOSService.GetAppIdentifier(settings.App);
                WriteConsoleOutput($"    App bundle: '[green]{Markup.Escape(settings.App)}[/]'", settings);
                WriteConsoleOutput($"    Bundle identifier found: '[green]{Markup.Escape(bundleIdentifier)}[/]'", settings);
            }
            else
            {
                var result = new AppLaunchResult
                {
                    Success = false,
                    ErrorMessage = "Either --app or --bundle-id must be specified"
                };

                WriteConsoleOutput($"[red]Error: Either --app or --bundle-id must be specified[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            // Check if app is installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (!await iOSService.IsAppInstalledAsync(bundleIdentifier, settings.Device))
            {
                WriteConsoleOutput($"[red]Error: The application is not installed[/]", settings);
                var result = new AppLaunchResult
                {
                    Success = false,
                    AppIdentity = bundleIdentifier,
                    ErrorMessage = "The application is not installed"
                };
                WriteResult(result, settings);
                return 1;
            }

            WriteConsoleOutput($"    App is installed.", settings);

            // Launch the app
            WriteConsoleOutput($"  - Launching app: '[green]{Markup.Escape(bundleIdentifier)}[/]'", settings);
            await iOSService.LaunchAppAsync(bundleIdentifier, settings.Device);
            WriteConsoleOutput($"    App launched successfully.", settings);

            var launchResult = new AppLaunchResult
            {
                Success = true,
                AppIdentity = bundleIdentifier
            };

            WriteResult(launchResult, settings);
            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppLaunchResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppIdentity = settings.BundleId
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}
