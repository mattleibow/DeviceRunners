using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AppUninstallCommand(IAnsiConsole console) : BaseCommand<AppUninstallCommand.Settings>(console)
{
	public class Settings : BaseCommandSettings
    {
        [Description("Path to the MSIX application package (to determine app identity)")]
        [CommandOption("--app")]
        public string? App { get; set; }

        [Description("App identity name to uninstall")]
        [CommandOption("--identity")]
        public string? Identity { get; set; }

        [Description("Certificate fingerprint to uninstall after package uninstall")]
        [CommandOption("--certificate-fingerprint")]
        public string? CertificateFingerprint { get; set; }
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
                var result = new AppUninstallResult
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
            if (packageService.IsPackageInstalled(appIdentity))
            {
                WriteConsoleOutput($"    App was installed, uninstalling...", settings);
                packageService.UninstallPackage(appIdentity);
                WriteConsoleOutput($"    Uninstall complete.", settings);
            }
            else
            {
                WriteConsoleOutput($"    App was not installed.", settings);
            }

            // Uninstall certificate if specified
            if (!string.IsNullOrEmpty(settings.CertificateFingerprint))
            {
                var certificateService = new CertificateService();
                WriteConsoleOutput($"  - Removing certificate...", settings);
                try
                {
                    certificateService.UninstallCertificate(settings.CertificateFingerprint);
                    WriteConsoleOutput($"    Certificate removed.", settings);
                }
                catch (Exception ex)
                {
                    WriteConsoleOutput($"    [yellow]Warning: Failed to remove certificate: {Markup.Escape(ex.Message)}[/]", settings);
                }
            }

            var successResult = new AppUninstallResult
            {
                Success = true,
                AppIdentity = appIdentity
            };
            WriteResult(successResult, settings);

            return 0;
        }
        catch (Exception ex)
        {
            var result = new AppUninstallResult
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