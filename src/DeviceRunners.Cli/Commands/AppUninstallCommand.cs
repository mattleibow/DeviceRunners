using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AppUninstallCommand : BaseCommand<AppUninstallCommand.Settings>
{
    public AppUninstallCommand(IAnsiConsole console) : base(console)
    {
    }

    public class Settings : BaseCommandSettings
    {
        [Description("Path to the MSIX application package (to determine app identity)")]
        [CommandOption("--app")]
        public string? App { get; set; }

        [Description("App identity name to uninstall")]
        [CommandOption("--identity")]
        public string? Identity { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var appService = new AppService();
            string appIdentity;

            if (!string.IsNullOrEmpty(settings.Identity))
            {
                appIdentity = settings.Identity;
            }
            else if (!string.IsNullOrEmpty(settings.App))
            {
                WriteConsoleOutput("  - Determining app identity from MSIX...", settings);
                appIdentity = appService.GetAppIdentityFromMsix(settings.App);
                WriteConsoleOutput($"    App identity found: '[green]{appIdentity}[/]'", settings);
            }
            else
            {
                var result = new AppUninstallResult
                {
                    Success = false,
                    ErrorMessage = "Either --app or --identity must be specified"
                };

                WriteConsoleOutput("[red]Error: Either --app or --identity must be specified[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            // Check if app is installed
            WriteConsoleOutput("  - Testing to see if the app is installed...", settings);
            if (appService.IsAppInstalled(appIdentity))
            {
                WriteConsoleOutput($"    App was installed, uninstalling...", settings);
                appService.UninstallApp(appIdentity);
                WriteConsoleOutput("    Uninstall complete.", settings);
            }
            else
            {
                WriteConsoleOutput("    App was not installed.", settings);
            }

            var successResult = new AppUninstallResult
            {
                Success = true,
                AppIdentity = appIdentity
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

            WriteConsoleOutput($"[red]Error: {ex.Message}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}