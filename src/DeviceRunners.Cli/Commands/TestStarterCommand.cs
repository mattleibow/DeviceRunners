using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class TestStarterCommand : BaseCommand<TestStarterCommand.Settings>
{
    public TestStarterCommand(IAnsiConsole console) : base(console)
    {
    }

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
            WriteConsoleOutput("[blue]============================================================[/]", settings);
            WriteConsoleOutput("[blue]PREPARATION[/]", settings);
            WriteConsoleOutput("[blue]============================================================[/]", settings);

            var appService = new AppService();
            var certificateService = new CertificateService();

            // Determine certificate
            var certificatePath = settings.Certificate ?? appService.GetCertificateFromMsix(settings.App);
            var certFingerprint = appService.GetCertificateFingerprint(certificatePath);
            
            WriteConsoleOutput("  - Determining certificate for MSIX installer...", settings);
            WriteConsoleOutput($"    File path: '[green]{certificatePath}[/]'", settings);
            WriteConsoleOutput($"    Thumbprint: '[green]{certFingerprint}[/]'", settings);
            WriteConsoleOutput("    Certificate identified.", settings);

            // Determine app identity
            WriteConsoleOutput("  - Determining app identity...", settings);
            var appIdentity = appService.GetAppIdentityFromMsix(settings.App);
            WriteConsoleOutput($"    MSIX installer: '[green]{settings.App}[/]'", settings);
            WriteConsoleOutput($"    App identity found: '[green]{appIdentity}[/]'", settings);

            // Check if app is already installed
            WriteConsoleOutput("  - Testing to see if the app is installed...", settings);
            if (appService.IsAppInstalled(appIdentity))
            {
                WriteConsoleOutput($"    App was installed, uninstalling...", settings);
                appService.UninstallApp(appIdentity);
                WriteConsoleOutput("    Uninstall complete...", settings);
            }
            else
            {
                WriteConsoleOutput("    App was not installed.", settings);
            }

            // Check certificate installation
            WriteConsoleOutput("  - Testing available certificates...", settings);
            if (!appService.IsCertificateInstalled(certFingerprint))
            {
                WriteConsoleOutput("    Certificate was not found, importing certificate...", settings);
                appService.InstallCertificate(certificatePath);
                WriteConsoleOutput("    Certificate imported.", settings);
            }
            else
            {
                WriteConsoleOutput("    Certificate was found.", settings);
            }

            // Install the app
            WriteConsoleOutput("  - Installing the app...", settings);
            appService.InstallApp(settings.App);
            WriteConsoleOutput("    Application installed.", settings);

            // Start the app
            WriteConsoleOutput("  - Starting the application...", settings);
            appService.StartApp(appIdentity, null);
            WriteConsoleOutput("    Application started.", settings);

            // Handle TCP test results
            var (testFailures, testResults) = await StartTestListener(settings);

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

            WriteConsoleOutput($"[red]Error: {ex.Message}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }

    private async Task<(int testFailures, string? testResults)> StartTestListener(Settings settings)
    {
        // Ensure artifacts directory exists for TCP results
        Directory.CreateDirectory(settings.ResultsDirectory);

        WriteConsoleOutput("  - Starting TCP listener on port 16384...", settings);
        var tcpResultsFile = Path.Combine(settings.ResultsDirectory, "tcp-test-results.txt");

        WriteConsoleOutput("  - Waiting for test results via TCP...", settings);
        WriteConsoleOutput("[blue]------------------------------------------------------------[/]", settings);

        var networkService = new NetworkService();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10)); // 10 minute timeout

        try
        {
            var results = await networkService.StartTcpListener(16384, tcpResultsFile, true, cancellationTokenSource.Token);
            
            WriteConsoleOutput("[blue]------------------------------------------------------------[/]", settings);
            
            if (File.Exists(tcpResultsFile))
            {
                var tcpResults = await File.ReadAllTextAsync(tcpResultsFile);
                WriteConsoleOutput("  - Analyzing TCP test results...", settings);
                
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
                                WriteConsoleOutput("    TCP results indicate no test failures.", settings);
                                return (0, tcpResults);
                            }
                        }
                    }
                }
                else
                {
                    WriteConsoleOutput("    [yellow]Could not parse test results format.[/]", settings);
                    return (1, tcpResults);
                }
            }
            else
            {
                WriteConsoleOutput("    [yellow]No TCP results received.[/]", settings);
                return (1, null);
            }
        }
        catch (OperationCanceledException)
        {
            WriteConsoleOutput("    [yellow]TCP listener timed out waiting for results.[/]", settings);
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

