using System.ComponentModel;
using System.Xml.Linq;
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

        [Description("Testing mode")]
        [CommandOption("--testing-mode")]
        public TestingMode? TestingMode { get; set; }
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
            var launchArgs = GetLaunchArguments(settings);
            appService.StartApp(appIdentity, launchArgs);
            AnsiConsole.MarkupLine("    Application started.");

            // Handle special testing modes
            if (settings.TestingMode == Commands.TestingMode.NonInteractiveVisual)
            {
                await HandleNonInteractiveVisualMode(settings.OutputDirectory);
            }
            else if (settings.TestingMode == Commands.TestingMode.XHarness)
            {
                await HandleXHarnessMode(settings.OutputDirectory);
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    private string? GetLaunchArguments(Settings settings)
    {
        return settings.TestingMode switch
        {
            Commands.TestingMode.XHarness => "--xharness --output-directory=\"" + settings.OutputDirectory + "\"",
            _ => null
        };
    }

    private async Task HandleXHarnessMode(string outputDirectory)
    {
        AnsiConsole.MarkupLine("  - Waiting for test results...");
        AnsiConsole.MarkupLine("[blue]------------------------------------------------------------[/]");

        // Wait for the tests to finish by monitoring TestResults.xml
        var testResultsPath = Path.Combine(outputDirectory, "TestResults.xml");
        var lastLineCount = 0;

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(15)); // 15 minute timeout
        
        try
        {
            while (!File.Exists(testResultsPath) && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(600, cancellationTokenSource.Token); // Match PowerShell's 0.6 second delay
                
                // Look for log files and stream their content
                var logFiles = Directory.GetFiles(outputDirectory, "test-output-*.log");
                foreach (var logFile in logFiles)
                {
                    try
                    {
                        var lines = await File.ReadAllLinesAsync(logFile, cancellationTokenSource.Token);
                        if (lines.Length > lastLineCount)
                        {
                            // Display new lines
                            for (int i = lastLineCount; i < lines.Length; i++)
                            {
                                AnsiConsole.WriteLine(lines[i]);
                            }
                            lastLineCount = lines.Length;
                        }
                    }
                    catch (IOException)
                    {
                        // File might be locked, skip this iteration
                        continue;
                    }
                }
            }

            AnsiConsole.MarkupLine("[blue]------------------------------------------------------------[/]");
            
            if (File.Exists(testResultsPath))
            {
                AnsiConsole.MarkupLine("  - Checking test results for failures...");
                AnsiConsole.MarkupLine($"    Results file: '{testResultsPath}'");
                
                var resultsXml = await File.ReadAllTextAsync(testResultsPath);
                var xmlDoc = System.Xml.Linq.XDocument.Parse(resultsXml);
                
                var hasFailures = xmlDoc.Descendants("assembly")
                    .Any(assembly => 
                    {
                        var failed = assembly.Attribute("failed")?.Value;
                        var error = assembly.Attribute("error")?.Value;
                        return (int.TryParse(failed, out int failedCount) && failedCount > 0) ||
                               (int.TryParse(error, out int errorCount) && errorCount > 0);
                    });
                
                if (hasFailures)
                {
                    AnsiConsole.MarkupLine("    [red]There were test failures.[/]");
                    Environment.ExitCode = 1;
                }
                else
                {
                    AnsiConsole.MarkupLine("    [green]There were no test failures.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("    [yellow]TestResults.xml not found within timeout period.[/]");
                Environment.ExitCode = 1;
            }
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("    [yellow]XHarness test monitoring timed out.[/]");
            Environment.ExitCode = 1;
        }
        
        AnsiConsole.MarkupLine("  - Tests complete.");
    }

    private async Task HandleNonInteractiveVisualMode(string outputDirectory)
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

public enum TestingMode
{
    XHarness,
    NonInteractiveVisual,
    None
}