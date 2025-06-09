using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class TestCommand(IAnsiConsole console) : BaseCommand<TestCommand.Settings>(console)
{
	public class Settings : BaseCommandSettings
    {
        [Description("Path to the MSIX application package")]
        [CommandOption("--app")]
        public required string App { get; set; }

        [Description("Path to the certificate file")]
        [CommandOption("--certificate")]
        public string? Certificate { get; set; }

        [Description("Results directory for test outputs")]
        [CommandOption("--results-directory")]
        [DefaultValue("artifacts")]
        public string ResultsDirectory { get; set; } = "artifacts";


    }

    public override int Execute(CommandContext context, Settings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]PREPARATION[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

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

    private async Task<(int testFailures, string? testResults)> StartTestListener(Settings settings)
    {
        // Ensure artifacts directory exists for TCP results
        Directory.CreateDirectory(settings.ResultsDirectory);

        WriteConsoleOutput($"  - Starting TCP listener on port 16384...", settings);
        var tcpResultsFile = Path.Combine(settings.ResultsDirectory, "tcp-test-results.txt");

        WriteConsoleOutput($"  - Waiting for test results via TCP...", settings);
        WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

        var networkService = new NetworkService();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10)); // 10 minute timeout

        try
        {
            var results = await networkService.StartTcpListener(16384, tcpResultsFile, true, 30, 30, cancellationTokenSource.Token);
            
            WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);
            
            if (File.Exists(tcpResultsFile))
            {
                var tcpResults = await File.ReadAllTextAsync(tcpResultsFile);
                WriteConsoleOutput($"  - Analyzing TCP test results...", settings);
                
                // Look for test failure indicators in the TCP results
                if (tcpResults.Contains("Failed:"))
                {
                    var lines = tcpResults.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains("Failed:") && int.TryParse(ExtractNumber(line, "Failed:"), out int failedCount))
                        {
                            if (failedCount > 0)
                            {
                                WriteConsoleOutput($"    TCP results indicate {failedCount} test failures.", settings);
                                return (failedCount, tcpResults);
                            }
                            else
                            {
                                WriteConsoleOutput($"    TCP results indicate no test failures.", settings);
                                return (0, tcpResults);
                            }
                        }
                    }
                }
                else
                {
                    WriteConsoleOutput($"    [yellow]Could not parse test results format.[/]", settings);
                    return (1, tcpResults);
                }
            }
            else
            {
                WriteConsoleOutput($"    [yellow]No TCP results received.[/]", settings);
                return (1, null);
            }
        }
        catch (OperationCanceledException)
        {
            WriteConsoleOutput($"    [yellow]TCP listener timed out waiting for results.[/]", settings);
            return (1, null);
        }

        return (1, null);
    }

    private string ExtractNumber(string text, string prefix)
    {
        var index = text.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            var start = index + prefix.Length;
            var end = start;
            while (end < text.Length && (char.IsDigit(text[end]) || char.IsWhiteSpace(text[end])))
            {
                end++;
            }
            return text.Substring(start, end - start).Trim();
        }
        return "0";
    }
}

