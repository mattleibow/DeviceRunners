using System.ComponentModel;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AppLaunchCommand : Command<AppLaunchCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to the MSIX application package (to determine app identity)")]
        [CommandOption("--app")]
        public string? App { get; set; }

        [Description("App identity name to launch")]
        [CommandOption("--identity")]
        public string? Identity { get; set; }

        [Description("Launch arguments to pass to the application")]
        [CommandOption("--args")]
        public string? Arguments { get; set; }
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
                AnsiConsole.MarkupLine("  - Determining app identity from MSIX...");
                appIdentity = appService.GetAppIdentityFromMsix(settings.App);
                AnsiConsole.MarkupLine($"    App identity found: '[green]{appIdentity}[/]'");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error: Either --app or --identity must be specified[/]");
                return 1;
            }

            // Check if app is installed
            AnsiConsole.MarkupLine("  - Testing to see if the app is installed...");
            if (!appService.IsAppInstalled(appIdentity))
            {
                AnsiConsole.MarkupLine($"    [red]App is not installed: {appIdentity}[/]");
                return 1;
            }

            // Start the app
            AnsiConsole.MarkupLine("  - Starting the application...");
            appService.StartApp(appIdentity, settings.Arguments);
            AnsiConsole.MarkupLine("    Application started.");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}