using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AndroidAppUninstallCommand(IAnsiConsole console) : BaseCommand<AndroidAppUninstallCommand.Settings>(console)
{
    public class Settings : BaseCommandSettings
    {
        [Description("Path to the APK application package (to determine package name)")]
        [CommandOption("--app")]
        public string? App { get; set; }
        
        [Description("Package name to uninstall")]
        [CommandOption("--package")]
        public string? Package { get; set; }
        
        [Description("Device or emulator serial number")]
        [CommandOption("--device")]
        public string? Device { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var androidService = new AndroidService();
            string packageName;
            
            // Display device info if specified
            if (!string.IsNullOrEmpty(settings.Device))
            {
                WriteConsoleOutput($"  - Using device: [green]{Markup.Escape(settings.Device)}[/]", settings);
            }

            if (!string.IsNullOrEmpty(settings.Package))
            {
                packageName = settings.Package;
            }
            else if (!string.IsNullOrEmpty(settings.App))
            {
                WriteConsoleOutput($"  - Determining package name from APK...", settings);
                packageName = androidService.GetPackageName(settings.App);
                WriteConsoleOutput($"    APK file: '[green]{Markup.Escape(settings.App)}[/]'", settings);
                WriteConsoleOutput($"    Package name found: '[green]{Markup.Escape(packageName)}[/]'", settings);
            }
            else
            {
                var result = new AppUninstallResult
                {
                    Success = false,
                    ErrorMessage = "Either --app or --package must be specified"
                };

                WriteConsoleOutput($"[red]Error: Either --app or --package must be specified[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            // Check if app is installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (androidService.IsPackageInstalled(packageName, settings.Device))
            {
                WriteConsoleOutput($"    App is installed, proceeding with uninstall...", settings);
                androidService.UninstallApk(packageName, settings.Device);
                WriteConsoleOutput($"    Uninstall complete.", settings);
            }
            else
            {
                WriteConsoleOutput($"    App is not installed, nothing to uninstall.", settings);
            }

            var successResult = new AppUninstallResult
            {
                Success = true,
                AppIdentity = packageName
            };

            WriteResult(successResult, settings);
            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppUninstallResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}
