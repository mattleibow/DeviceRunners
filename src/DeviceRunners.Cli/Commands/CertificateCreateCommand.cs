using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class CertificateCreateCommand : Command<CertificateCreateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Publisher identity for the certificate")]
        [CommandOption("--publisher")]
        public string? Publisher { get; set; }

        [Description("Path to Package.appxmanifest file")]
        [CommandOption("--manifest")]
        public string? Manifest { get; set; }

        [Description("Path to project directory")]
        [CommandOption("--project")]
        public string? Project { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine("[blue]============================================================[/]");
            AnsiConsole.MarkupLine("[blue]PREPARATION[/]");
            AnsiConsole.MarkupLine("[blue]============================================================[/]");

            var publisher = DeterminePublisher(settings);
            
            AnsiConsole.MarkupLine("  - Preparation complete.");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[blue]============================================================[/]");
            AnsiConsole.MarkupLine("[blue]GENERATE CERTIFICATE[/]");
            AnsiConsole.MarkupLine("[blue]============================================================[/]");

            var certificateService = new CertificateService();
            var fingerprint = certificateService.CreateSelfSignedCertificate(publisher);

            AnsiConsole.MarkupLine($"    Publisher: '[green]{publisher}[/]'");
            AnsiConsole.MarkupLine($"    Thumbprint: '[green]{fingerprint}[/]'");
            AnsiConsole.MarkupLine("    Certificate generated.");
            AnsiConsole.MarkupLine("  - Generation complete.");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine($"[green]{fingerprint}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    private string DeterminePublisher(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.Publisher))
        {
            AnsiConsole.MarkupLine($"  - Publisher identity provided: '[green]{settings.Publisher}[/]'");
            return settings.Publisher;
        }

        AnsiConsole.MarkupLine("  - Determining publisher identity...");

        var manifestPath = DetermineManifestPath(settings);
        
        AnsiConsole.MarkupLine($"    Reading publisher identity from the manifest: '[green]{manifestPath}[/]'...");
        
        var manifestXml = XDocument.Load(manifestPath);
        var publisher = manifestXml.Root?.Element(XName.Get("Identity", "http://schemas.microsoft.com/appx/manifest/foundation/windows10"))?.Attribute("Publisher")?.Value;
        
        if (string.IsNullOrEmpty(publisher))
        {
            throw new InvalidOperationException("Unable to read publisher identity from manifest.");
        }

        AnsiConsole.MarkupLine($"    Publisher identity: '[green]{publisher}[/]'");
        return publisher;
    }

    private string DetermineManifestPath(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.Manifest))
        {
            if (!File.Exists(settings.Manifest))
            {
                throw new FileNotFoundException($"Invalid manifest provided: '{settings.Manifest}'.");
            }
            return Path.GetFullPath(settings.Manifest);
        }

        if (string.IsNullOrEmpty(settings.Project))
        {
            throw new InvalidOperationException("No parameters were provided. Provide either the --publisher or --manifest values.");
        }

        AnsiConsole.MarkupLine($"    No manifest was provided, trying to use the project '[green]{settings.Project}[/]'...");

        var possiblePaths = new[]
        {
            Path.Combine(settings.Project, "..", "Package.appxmanifest"),
            Path.Combine(settings.Project, "..", "Platforms", "Windows", "Package.appxmanifest")
        };

        foreach (var possible in possiblePaths)
        {
            var resolvedPath = Path.GetFullPath(possible);
            AnsiConsole.MarkupLine($"    Trying the manifest path '[yellow]{possible}[/]'...");
            
            if (File.Exists(resolvedPath))
            {
                AnsiConsole.MarkupLine($"    Manifest found: '[green]{resolvedPath}[/]'");
                return resolvedPath;
            }
        }

        throw new FileNotFoundException("Unable to locate the Package.appxmanifest. Provide either the --publisher or --manifest values.");
    }
}