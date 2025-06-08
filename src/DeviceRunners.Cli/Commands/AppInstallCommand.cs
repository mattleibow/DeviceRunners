using System.ComponentModel;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AppInstallCommand : Command<AppInstallCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to the MSIX application package")]
        [CommandOption("--app")]
        public required string App { get; set; }

        [Description("Path to the certificate file")]
        [CommandOption("--certificate")]
        public string? Certificate { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        try
        {
            var appService = new AppService();

            // Determine certificate
            var certificatePath = settings.Certificate ?? appService.GetCertificateFromMsix(settings.App);
            var certFingerprint = appService.GetCertificateFingerprint(certificatePath);
            
            AnsiConsole.MarkupLine("  - Determining certificate for MSIX installer...");
            AnsiConsole.MarkupLine($"    File path: '[green]{certificatePath}[/]'");
            AnsiConsole.MarkupLine($"    Thumbprint: '[green]{certFingerprint}[/]'");
            AnsiConsole.MarkupLine("    Certificate identified.");

            // Determine app identity
            AnsiConsole.MarkupLine("  - Determining app identity...");
            var appIdentity = appService.GetAppIdentityFromMsix(settings.App);
            AnsiConsole.MarkupLine($"    MSIX installer: '[green]{settings.App}[/]'");
            AnsiConsole.MarkupLine($"    App identity found: '[green]{appIdentity}[/]'");

            // Check if app is already installed
            AnsiConsole.MarkupLine("  - Testing to see if the app is installed...");
            if (appService.IsAppInstalled(appIdentity))
            {
                AnsiConsole.MarkupLine($"    App was already installed, uninstalling first...");
                appService.UninstallApp(appIdentity);
                AnsiConsole.MarkupLine("    Uninstall complete...");
            }
            else
            {
                AnsiConsole.MarkupLine("    App was not installed.");
            }

            // Check certificate installation
            AnsiConsole.MarkupLine("  - Testing available certificates...");
            if (!appService.IsCertificateInstalled(certFingerprint))
            {
                AnsiConsole.MarkupLine("    Certificate was not found, importing certificate...");
                appService.InstallCertificate(certificatePath);
                AnsiConsole.MarkupLine("    Certificate imported.");
            }
            else
            {
                AnsiConsole.MarkupLine("    Certificate was found.");
            }

            // Install the app
            AnsiConsole.MarkupLine("  - Installing the app...");
            appService.InstallApp(settings.App);
            AnsiConsole.MarkupLine("    Application installed.");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}