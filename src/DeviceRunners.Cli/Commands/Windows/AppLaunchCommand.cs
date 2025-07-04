using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WindowsAppLaunchCommand(IAnsiConsole console) : BaseCommand<WindowsAppLaunchCommand.Settings>(console)
{
	public class Settings : BaseCommandSettings
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
            var packageService = new PackageService();
            string appIdentity;

            if (!string.IsNullOrEmpty(settings.Identity))
            {
                appIdentity = settings.Identity;
            }
            else if (!string.IsNullOrEmpty(settings.App))
            {
                WriteConsoleOutput($"  - Determining app identity from MSIX...", settings);
                appIdentity = packageService.GetPackageIdentity(settings.App);
                WriteConsoleOutput($"    App identity found: '[green]{Markup.Escape(appIdentity)}[/]'", settings);
            }
            else
            {
                var result = new AppLaunchResult
                {
                    Success = false,
                    ErrorMessage = "Either --app or --identity must be specified"
                };

                WriteConsoleOutput($"[red]Error: Either --app or --identity must be specified[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            // Check if app is installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (!packageService.IsPackageInstalled(appIdentity))
            {
                var result = new AppLaunchResult
                {
                    Success = false,
                    ErrorMessage = $"App is not installed: {appIdentity}",
                    AppIdentity = appIdentity
                };

                WriteConsoleOutput($"    [red]App is not installed: {Markup.Escape(appIdentity)}[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            // Start the app
            WriteConsoleOutput($"  - Starting the application...", settings);
            packageService.LaunchApp(appIdentity, settings.Arguments);
            WriteConsoleOutput($"    Application started.", settings);

            var successResult = new AppLaunchResult
            {
                Success = true,
                AppIdentity = appIdentity,
                Arguments = settings.Arguments
            };
            WriteResult(successResult, settings);

            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppLaunchResult
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