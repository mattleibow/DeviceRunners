using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class MacOSAppInstallCommand(IAnsiConsole console) : BaseCommand<MacOSAppInstallCommand.Settings>(console)
{
    public class Settings : BaseCommandSettings
    {
        [Description("Path to the .app application bundle")]
        [CommandOption("--app")]
        public required string App { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var macOSService = new MacOSService();

            WriteConsoleOutput($"  - Installing app bundle: [green]{Markup.Escape(settings.App)}[/]", settings);
            macOSService.InstallApp(settings.App);
            WriteConsoleOutput($"    App bundle installed successfully.", settings);

            var result = new AppInstallResult
            {
                Success = true,
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