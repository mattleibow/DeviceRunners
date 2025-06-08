using System.ComponentModel;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class TestStarterCommand : Command<TestStarterCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to the MSIX application package")]
        [CommandOption("--app")]
        public required string App { get; set; }

        [Description("Path to the certificate file")]
        [CommandOption("--certificate")]
        public string? Certificate { get; set; }

        [Description("Output directory for test results")]
        [CommandOption("--output-directory")]
        [DefaultValue("artifacts")]
        public string OutputDirectory { get; set; } = "artifacts";


    }

    public override int Execute(CommandContext context, Settings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine("[blue]============================================================[/]");
            AnsiConsole.MarkupLine("[blue]PREPARATION[/]");
            AnsiConsole.MarkupLine("[blue]============================================================[/]");

            var appService = new AppService();
            var certificateService = new CertificateService();

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
                AnsiConsole.MarkupLine($"    App was installed, uninstalling...");
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

            // Start the app
            AnsiConsole.MarkupLine("  - Starting the application...");
            appService.StartApp(appIdentity, null);
            AnsiConsole.MarkupLine("    Application started.");

            // Handle TCP test results
            await StartTestListener(settings.OutputDirectory);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    private async Task StartTestListener(string outputDirectory)
    {
        // Ensure artifacts directory exists for TCP results
        Directory.CreateDirectory(outputDirectory);

        AnsiConsole.MarkupLine("  - Starting TCP listener on port 16384...");
        var tcpResultsFile = Path.Combine(outputDirectory, "tcp-test-results.txt");

        AnsiConsole.MarkupLine("  - Waiting for test results via TCP...");
        AnsiConsole.MarkupLine("[blue]------------------------------------------------------------[/]");

        var networkService = new NetworkService();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10)); // 10 minute timeout

        try
        {
            var results = await networkService.StartTcpListener(16384, tcpResultsFile, true, cancellationTokenSource.Token);
            
            AnsiConsole.MarkupLine("[blue]------------------------------------------------------------[/]");
            
            if (File.Exists(tcpResultsFile))
            {
                var tcpResults = await File.ReadAllTextAsync(tcpResultsFile);
                AnsiConsole.MarkupLine("  - Analyzing TCP test results...");
                
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
                                AnsiConsole.MarkupLine($"    TCP results indicate {failedCount} test failures.");
                                Environment.ExitCode = 1;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("    TCP results indicate no test failures.");
                            }
                            break;
                        }
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("    [yellow]Could not parse test results format.[/]");
                    Environment.ExitCode = 1;
                }
            }
            else
            {
                AnsiConsole.MarkupLine("    [yellow]No TCP results received.[/]");
                Environment.ExitCode = 1;
            }
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("    [yellow]TCP listener timed out waiting for results.[/]");
            Environment.ExitCode = 1;
        }
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

