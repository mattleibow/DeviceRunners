using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class CertificateRemoveCommand(IAnsiConsole console) : BaseCommand<CertificateRemoveCommand.Settings>(console)
{
	public class Settings : BaseCommandSettings
    {
        [Description("Certificate fingerprint to remove")]
        [CommandOption("--fingerprint")]
        public required string Fingerprint { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]REMOVE CERTIFICATE[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            var certificateService = new CertificateService();
            
            WriteConsoleOutput($"  - Testing available certificates...", settings);
            
            bool wasFound = certificateService.CertificateExists(settings.Fingerprint);
            bool removed = false;

            if (wasFound)
            {
                WriteConsoleOutput($"    Certificate was found.", settings);
                WriteConsoleOutput($"  - Removing certificate with fingerprint '[yellow]{settings.Fingerprint}[/]'...", settings);
                
                removed = certificateService.RemoveCertificate(settings.Fingerprint);
                if (removed)
                {
                    WriteConsoleOutput($"    Certificate removed.", settings);
                }
                else
                {
                    WriteConsoleOutput($"[yellow]    Failed to remove certificate.[/]", settings);
                }
            }
            else
            {
                WriteConsoleOutput($"    Certificate was not found.", settings);
            }

            WriteConsoleLine(settings);

            var result = new CertificateRemoveResult
            {
                Success = !wasFound || removed, // Success if not found or successfully removed
                Fingerprint = settings.Fingerprint,
                WasFound = wasFound
            };
            WriteResult(result, settings);

            return 0;
        }
        catch (Exception ex)
        {
            var result = new CertificateRemoveResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Fingerprint = settings.Fingerprint,
                WasFound = false
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}