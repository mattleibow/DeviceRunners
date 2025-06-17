using System.ComponentModel;
using System.Diagnostics;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WindowsTestCommand(IAnsiConsole console) : BaseTestCommand<WindowsTestCommand.Settings>(console)
{
	public class Settings : BaseTestCommandSettings
    {
        [Description("Path to the certificate file")]
        [CommandOption("--certificate")]
        public string? Certificate { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]PREPARATION[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            // Check if this is an unpackaged app (.exe) or packaged app (.msix)
            if (string.IsNullOrEmpty(settings.App))
            {
                throw new ArgumentException("Application path is required. Use --app to specify the path to your application.");
            }

            var extension = Path.GetExtension(settings.App).ToLowerInvariant();
            if (extension == ".exe")
            {
                return await ExecuteUnpackagedApp(settings);
            }
            else
            {
                return await ExecutePackagedApp(settings);
            }
        }
        catch (Exception ex)
        {
            var result = new TestStartResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }

    private async Task<int> ExecuteUnpackagedApp(Settings settings)
    {
        WriteConsoleOutput($"  - Validating unpackaged application...", settings);
        
        // Validate that the app file exists and is an executable
        if (!File.Exists(settings.App))
        {
            throw new FileNotFoundException($"Application file not found: {settings.App}");
        }

        WriteConsoleOutput($"    Application file: '[green]{Markup.Escape(settings.App)}[/]'", settings);
        WriteConsoleOutput($"    Application validated.", settings);

        WriteConsoleOutput($"", settings);
        WriteConsoleOutput($"[blue]============================================================[/]", settings);
        WriteConsoleOutput($"[blue]EXECUTION[/]", settings);
        WriteConsoleOutput($"[blue]============================================================[/]", settings);

        // Start the unpackaged application directly
        WriteConsoleOutput($"  - Starting the unpackaged application...", settings);
        
        var startInfo = new ProcessStartInfo
        {
            FileName = settings.App,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(settings.App)
        };

        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start the application process.");
        }

        WriteConsoleOutput($"    Application started with PID: {process.Id}", settings);

        // Handle TCP test results
        var (testFailures, testResults) = await StartTestListener(settings);

        WriteConsoleOutput($"", settings);
        WriteConsoleOutput($"[blue]============================================================[/]", settings);
        WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
        WriteConsoleOutput($"[blue]============================================================[/]", settings);

        // For unpackaged apps, we just try to terminate the process if it's still running
        WriteConsoleOutput($"  - Checking application process...", settings);
        try
        {
            if (!process.HasExited)
            {
                WriteConsoleOutput($"    Application is still running, terminating...", settings);
                process.Kill();
                process.WaitForExit(5000); // Wait up to 5 seconds for graceful exit
                WriteConsoleOutput($"    Application terminated.", settings);
            }
            else
            {
                WriteConsoleOutput($"    Application has already exited.", settings);
            }
        }
        catch (Exception ex)
        {
            WriteConsoleOutput($"    [yellow]Warning: Failed to check/terminate application process: {Markup.Escape(ex.Message)}[/]", settings);
        }

        WriteConsoleOutput($"  - Cleanup complete.", settings);

        var result = new TestStartResult
        {
            Success = testFailures == 0,
            AppPath = settings.App,
            ResultsDirectory = settings.ResultsDirectory,
            TestFailures = testFailures,
            TestResults = testResults
        };
        WriteResult(result, settings);

        return testFailures > 0 ? 1 : 0;
    }

    private async Task<int> ExecutePackagedApp(Settings settings)
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
            WriteConsoleOutput($"    App was installed, uninstalling...", settings);
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

        // Install dependencies first
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

        // Install the app
        WriteConsoleOutput($"  - Installing the app...", settings);
        packageService.InstallPackage(settings.App);
        WriteConsoleOutput($"    Application installed.", settings);

        // Start the app
        WriteConsoleOutput($"  - Starting the application...", settings);
        packageService.LaunchApp(appIdentity, null);
        WriteConsoleOutput($"    Application started.", settings);

        // Handle TCP test results
        var (testFailures, testResults) = await StartTestListener(settings);

        WriteConsoleOutput($"", settings);
        WriteConsoleOutput($"[blue]============================================================[/]", settings);
        WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
        WriteConsoleOutput($"[blue]============================================================[/]", settings);

        // Cleanup: Uninstall the app
        WriteConsoleOutput($"  - Uninstalling application...", settings);
        try
        {
            packageService.UninstallPackage(appIdentity);
            WriteConsoleOutput($"    Application uninstalled.", settings);
        }
        catch (Exception ex)
        {
            WriteConsoleOutput($"    [yellow]Warning: Failed to uninstall application: {Markup.Escape(ex.Message)}[/]", settings);
        }

        // Cleanup: Remove certificate if we auto-installed it
        if (autoInstalledCertificate)
        {
            WriteConsoleOutput($"  - Removing installed certificates...", settings);
            try
            {
                certificateService.UninstallCertificate(certFingerprint);
                WriteConsoleOutput($"    Installed certificates removed.", settings);
            }
            catch (Exception ex)
            {
                WriteConsoleOutput($"    [yellow]Warning: Failed to remove certificate: {Markup.Escape(ex.Message)}[/]", settings);
            }
        }

        WriteConsoleOutput($"  - Cleanup complete.", settings);

        var result = new TestStartResult
        {
            Success = testFailures == 0,
            AppIdentity = appIdentity,
            AppPath = settings.App,
            CertificateThumbprint = certFingerprint,
            ResultsDirectory = settings.ResultsDirectory,
            TestFailures = testFailures,
            TestResults = testResults
        };
        WriteResult(result, settings);

        return testFailures > 0 ? 1 : 0;
    }
}

