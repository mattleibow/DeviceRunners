using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WindowsAppInstallCommand(IAnsiConsole console) : BaseCommand<WindowsAppInstallCommand.Settings>(console)
{
	public class Settings : BaseCommandSettings
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
            var packageService = new PackageService();
            var certificateService = new CertificateService();

            // Determine certificate
            var certificatePath = settings.Certificate ?? packageService.GetCertificateFromMsix(settings.App);
            var certFingerprint = certificateService.GetCertificateFingerprint(certificatePath);
            
            WriteConsoleOutput($"  - Determining certificate for MSIX installer...", settings);
            WriteConsoleOutput($"    File path: '[green]{Markup.Escape(certificatePath)}[/]'", settings);
            WriteConsoleOutput($"    Thumbprint: '[green]{certFingerprint}[/]'", settings);
            WriteConsoleOutput($"    Certificate identified.", settings);

            // Determine app identity
            WriteConsoleOutput($"  - Determining app identity...", settings);
            var appIdentity = packageService.GetPackageIdentity(settings.App);
            WriteConsoleOutput($"    MSIX installer: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            WriteConsoleOutput($"    App identity found: '[green]{Markup.Escape(appIdentity)}[/]'", settings);

            // Check if app is already installed
            WriteConsoleOutput($"  - Testing to see if the app is installed...", settings);
            if (packageService.IsPackageInstalled(appIdentity))
            {
                WriteConsoleOutput($"    App was already installed, uninstalling first...", settings);
                packageService.UninstallPackage(appIdentity);
                WriteConsoleOutput($"    Uninstall complete...", settings);
            }
            else
            {
                WriteConsoleOutput($"    App was not installed.", settings);
            }

            // Check certificate installation
            var autoInstalledCertificate = false;
            WriteConsoleOutput($"  - Testing available certificates...", settings);
            if (!certificateService.IsCertificateInstalled(certFingerprint))
            {
                autoInstalledCertificate = true;
                WriteConsoleOutput($"    Certificate was not found, importing certificate...", settings);
                certificateService.InstallCertificate(certificatePath);
                WriteConsoleOutput($"    Certificate imported.", settings);
            }
            else
            {
                WriteConsoleOutput($"    Certificate was found.", settings);
            }

            // Install dependencies and the app
            WriteConsoleOutput($"  - Installing dependencies...", settings);
            var dependencies = packageService.GetDependencies(settings.App);
            foreach (var dependency in dependencies)
            {
                try
                {
                    WriteConsoleOutput($"    Installing dependency: '[green]{Markup.Escape(dependency)}[/]'", settings);
                    packageService.InstallPackage(dependency);
                }
                catch
                {
                    WriteConsoleOutput($"    Dependency failed to install, continuing...", settings);
                }
            }
            
            WriteConsoleOutput($"  - Installing application...", settings);
            packageService.InstallPackage(settings.App);
            WriteConsoleOutput($"    Application installed.", settings);

            var result = new AppInstallResult
            {
                Success = true,
                AppIdentity = appIdentity,
                AppPath = settings.App,
                CertificateThumbprint = certFingerprint,
                CertificateAutoInstalled = autoInstalledCertificate
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