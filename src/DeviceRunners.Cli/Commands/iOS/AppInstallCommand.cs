using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class iOSAppInstallCommand(IAnsiConsole console) : BaseAsyncCommand<iOSAppInstallCommand.Settings>(console)
{
    public class Settings : BaseAsyncCommandSettings
    {
        [Description("Path to the .app application bundle")]
        [CommandOption("--app")]
        public required string App { get; set; }
        
        [Description("iOS Simulator device ID (optional, will use booted simulator if not specified)")]
        [CommandOption("--device")]
        public string? Device { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var iOSService = new iOSService();

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
                    var result = new AppInstallResult
                    {
                        Success = false,
                        ErrorMessage = "No booted iOS simulator found. Please boot a simulator or specify a device ID with --device.",
                        AppPath = settings.App
                    };

                    WriteConsoleOutput($"[red]Error: No booted iOS simulator found. Please boot a simulator or specify a device ID with --device.[/]", settings);
                    WriteResult(result, settings);
                    return 1;
                }
                WriteConsoleOutput($"  - Using booted simulator: [green]{Markup.Escape(bootedDevice)}[/]", settings);
                settings.Device = bootedDevice;
            }

            // Determine app identifier
            WriteConsoleOutput($"  - Determining app identifier...", settings);
            string appIdentifier = iOSService.GetAppIdentifier(settings.App);
            WriteConsoleOutput($"    App bundle: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            WriteConsoleOutput($"    App identifier found: '[green]{Markup.Escape(appIdentifier)}[/]'", settings);

            // Check if app is already installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (await iOSService.IsAppInstalledAsync(appIdentifier, settings.Device))
            {
                WriteConsoleOutput($"    App was already installed, uninstalling first...", settings);
                await iOSService.UninstallAppAsync(appIdentifier, settings.Device);
                WriteConsoleOutput($"    Uninstall complete.", settings);
            }
            else
            {
                WriteConsoleOutput($"    App not installed, proceeding with new installation.", settings);
            }

            // Install the app
            WriteConsoleOutput($"  - Installing app: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            await iOSService.InstallAppAsync(settings.App, settings.Device);
            WriteConsoleOutput($"    App installed successfully.", settings);

            var installResult = new AppInstallResult
            {
                Success = true,
                AppIdentity = appIdentifier,
                AppPath = settings.App
            };

            WriteResult(installResult, settings);
            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppInstallResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppPath = settings.App
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}
