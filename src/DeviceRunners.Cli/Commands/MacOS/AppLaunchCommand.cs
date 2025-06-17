using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class MacOSAppLaunchCommand(IAnsiConsole console) : BaseCommand<MacOSAppLaunchCommand.Settings>(console)
{
    public class Settings : BaseCommandSettings
    {
        [Description("Path to the .app application bundle")]
        [CommandOption("--app")]
        public required string App { get; set; }

        [Description("Launch arguments to pass to the application")]
        [CommandOption("--args")]
        public string? Arguments { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var macOSService = new MacOSService();

            // Check if app is installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (!macOSService.IsAppInstalled(settings.App))
            {
                var result = new AppLaunchResult
                {
                    Success = false,
                    ErrorMessage = $"App is not installed: {settings.App}",
                    AppIdentity = Path.GetFileName(settings.App)
                };

                WriteConsoleOutput($"    [red]App is not installed: {Markup.Escape(settings.App)}[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            // Get app identifier
            WriteConsoleOutput($"  - Determining app identifier...", settings);
            var appIdentifier = macOSService.GetAppIdentifier(settings.App);
            WriteConsoleOutput($"    App bundle: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            WriteConsoleOutput($"    App identifier found: '[green]{Markup.Escape(appIdentifier)}[/]'", settings);

            // Start the app
            WriteConsoleOutput($"  - Starting the application...", settings);
            macOSService.LaunchApp(settings.App, settings.Arguments);
            WriteConsoleOutput($"    Application started.", settings);

            var successResult = new AppLaunchResult
            {
                Success = true,
                AppIdentity = appIdentifier
            };

            WriteResult(successResult, settings);
            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppLaunchResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppIdentity = Path.GetFileName(settings.App)
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}