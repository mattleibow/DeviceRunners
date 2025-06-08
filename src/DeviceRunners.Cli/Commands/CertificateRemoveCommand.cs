using System.ComponentModel;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class CertificateRemoveCommand : Command<CertificateRemoveCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Certificate fingerprint to remove")]
        [CommandOption("--fingerprint")]
        public required string Fingerprint { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine("[blue]============================================================[/]");
            AnsiConsole.MarkupLine("[blue]REMOVE CERTIFICATE[/]");
            AnsiConsole.MarkupLine("[blue]============================================================[/]");

            var certificateService = new CertificateService();
            
            AnsiConsole.MarkupLine("  - Testing available certificates...");
            
            if (certificateService.CertificateExists(settings.Fingerprint))
            {
                AnsiConsole.MarkupLine("    Certificate was found.");
                AnsiConsole.MarkupLine($"  - Removing certificate with fingerprint '[yellow]{settings.Fingerprint}[/]'...");
                
                if (certificateService.RemoveCertificate(settings.Fingerprint))
                {
                    AnsiConsole.MarkupLine("    Certificate removed.");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]    Failed to remove certificate.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("    Certificate was not found.");
            }

            AnsiConsole.WriteLine();
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}