using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AndroidAppInstallCommand(IAnsiConsole console) : BaseCommand<AndroidAppInstallCommand.Settings>(console)
{
    public class Settings : BaseCommandSettings
    {
        [Description("Path to the APK application package")]
        [CommandOption("--app")]
        public required string App { get; set; }
        
        [Description("Device or emulator serial number")]
        [CommandOption("--device")]
        public string? Device { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var androidService = new AndroidService();

            // Display device info if specified
            if (!string.IsNullOrEmpty(settings.Device))
            {
                WriteConsoleOutput($"  - Using device: [green]{Markup.Escape(settings.Device)}[/]", settings);
            }

            // Determine package name
            WriteConsoleOutput($"  - Determining package name from APK...", settings);
            string packageName = androidService.GetPackageName(settings.App);
            WriteConsoleOutput($"    APK file: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            WriteConsoleOutput($"    Package name found: '[green]{Markup.Escape(packageName)}[/]'", settings);

            // Check if app is already installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (androidService.IsPackageInstalled(packageName, settings.Device))
            {
                WriteConsoleOutput($"    App was already installed, uninstalling first...", settings);
                androidService.UninstallApk(packageName, settings.Device);
                WriteConsoleOutput($"    Uninstall complete.", settings);
            }
            else
            {
                WriteConsoleOutput($"    App not installed, proceeding with new installation.", settings);
            }

            // Install the APK
            WriteConsoleOutput($"  - Installing APK: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            androidService.InstallApk(settings.App, settings.Device);
            WriteConsoleOutput($"    APK installed successfully.", settings);

            var result = new AppInstallResult
            {
                Success = true,
                AppIdentity = packageName,
                AppPath = settings.App
            };

            WriteResult(result, settings);
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
