using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class MacOSAppUninstallCommand(IAnsiConsole console) : BaseCommand<MacOSAppUninstallCommand.Settings>(console)
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

            WriteConsoleOutput($"  - Uninstalling app bundle: [green]{Markup.Escape(settings.App)}[/]", settings);
            macOSService.UninstallApp(settings.App);
            WriteConsoleOutput($"    App bundle uninstalled successfully.", settings);

            var result = new AppUninstallResult
            {
                Success = true,
                AppIdentity = Path.GetFileName(settings.App)
            };

            WriteResult(result, settings);
            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppUninstallResult
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