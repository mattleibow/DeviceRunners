using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class CertificateCreateCommand : BaseCommand<CertificateCreateCommand.Settings>
{
    public CertificateCreateCommand(IAnsiConsole console) : base(console)
    {
    }

    public class Settings : BaseCommandSettings
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
            WriteConsoleOutput("[blue]============================================================[/]", settings);
            WriteConsoleOutput("[blue]PREPARATION[/]", settings);
            WriteConsoleOutput("[blue]============================================================[/]", settings);

            var publisher = DeterminePublisher(settings);
            
            WriteConsoleOutput("  - Preparation complete.", settings);
            WriteConsoleLine(settings);

            WriteConsoleOutput("[blue]============================================================[/]", settings);
            WriteConsoleOutput("[blue]GENERATE CERTIFICATE[/]", settings);
            WriteConsoleOutput("[blue]============================================================[/]", settings);

            var certificateService = new CertificateService();
            var fingerprint = certificateService.CreateSelfSignedCertificate(publisher);

            WriteConsoleOutput($"    Publisher: '[green]{publisher}[/]'", settings);
            WriteConsoleOutput($"    Thumbprint: '[green]{fingerprint}[/]'", settings);
            WriteConsoleOutput("    Certificate generated.", settings);
            WriteConsoleOutput("  - Generation complete.", settings);
            WriteConsoleLine(settings);
            WriteConsoleMarkup($"[green]{fingerprint}[/]", settings);

            // Write structured output if requested
            var result = new CertificateCreateResult
            {
                Success = true,
                Publisher = publisher,
                Thumbprint = fingerprint
            };
            WriteResult(result, settings);

            return 0;
        }
        catch (Exception ex)
        {
            var result = new CertificateCreateResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };

            WriteConsoleMarkup($"[red]Error: {ex.Message}[/]", settings);
            
            WriteResult(result, settings);
            return 1;
        }
    }

    private string DeterminePublisher(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.Publisher))
        {
            WriteConsoleOutput($"  - Publisher identity provided: '[green]{settings.Publisher}[/]'", settings);
            return settings.Publisher;
        }

        WriteConsoleOutput("  - Determining publisher identity...", settings);

        var manifestPath = DetermineManifestPath(settings);
        
        WriteConsoleOutput($"    Reading publisher identity from the manifest: '[green]{manifestPath}[/]'...", settings);
        
        var manifestXml = XDocument.Load(manifestPath);
        var publisher = manifestXml.Root?.Element(XName.Get("Identity", "http://schemas.microsoft.com/appx/manifest/foundation/windows10"))?.Attribute("Publisher")?.Value;
        
        if (string.IsNullOrEmpty(publisher))
        {
            throw new InvalidOperationException("Unable to read publisher identity from manifest.");
        }

        WriteConsoleOutput($"    Publisher identity: '[green]{publisher}[/]'", settings);
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

        WriteConsoleOutput($"    No manifest was provided, trying to use the project '[green]{settings.Project}[/]'...", settings);

        var possiblePaths = new[]
        {
            Path.Combine(settings.Project, "..", "Package.appxmanifest"),
            Path.Combine(settings.Project, "..", "Platforms", "Windows", "Package.appxmanifest")
        };

        foreach (var possible in possiblePaths)
        {
            var resolvedPath = Path.GetFullPath(possible);
            WriteConsoleOutput($"    Trying the manifest path '[yellow]{possible}[/]'...", settings);
            
            if (File.Exists(resolvedPath))
            {
                WriteConsoleOutput($"    Manifest found: '[green]{resolvedPath}[/]'", settings);
                return resolvedPath;
            }
        }

        throw new FileNotFoundException("Unable to locate the Package.appxmanifest. Provide either the --publisher or --manifest values.");
    }
}