using System.ComponentModel;

using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public abstract class BaseTestCommand<TSettings>(IAnsiConsole console) : BaseCommand<TSettings>(console)
    where TSettings : BaseTestCommand<TSettings>.BaseTestCommandSettings
{
    public abstract class BaseTestCommandSettings : BaseCommandSettings
    {
        [Description("Path to the application package")]
        [CommandOption("--app")]
        public required string App { get; set; }

        [Description("Results directory for test outputs")]
        [CommandOption("--results-directory")]
        [DefaultValue("artifacts")]
        public string ResultsDirectory { get; set; } = "artifacts";

        [Description("TCP port to listen on")]
        [CommandOption("--port")]
        [DefaultValue(16384)]
        public int Port { get; set; } = 16384;

        [Description("Connection timeout in seconds")]
        [CommandOption("--connection-timeout")]
        [DefaultValue(30)]
        public int ConnectionTimeout { get; set; } = 30;

        [Description("Data timeout in seconds")]
        [CommandOption("--data-timeout")]
        [DefaultValue(30)]
        public int DataTimeout { get; set; } = 30;
    }

    public override int Execute(CommandContext context, TSettings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    protected abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings);

    protected async Task<(int testFailures, string? testResults)> StartTestListener(TSettings settings)
    {
        // Ensure artifacts directory exists for TCP results
        Directory.CreateDirectory(settings.ResultsDirectory);

        WriteConsoleOutput($"  - Starting TCP listener on port {settings.Port}...", settings);
        var tcpResultsFile = Path.Combine(settings.ResultsDirectory, "tcp-test-results.txt");
        WriteConsoleOutput($"    Saving results to: [green]{Markup.Escape(tcpResultsFile)}[/].", settings);

        WriteConsoleOutput($"  - Waiting for test results via TCP...", settings);
        WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

        var networkService = new NetworkService();

        networkService.ConnectionEstablished += (sender, e) =>
        {
            WriteConsoleOutput($"    [yellow]TCP connection established with {e.RemoteEndPoint}[/]", settings);
        };
        networkService.ConnectionClosed += (sender, e) =>
        {
            WriteConsoleOutput($"    [yellow]TCP connection closed with {e.RemoteEndPoint}[/]", settings);
        };
        networkService.DataReceived += (sender, e) =>
        {
            foreach (var line in e.Data.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    WriteConsoleOutput($"    [green]Received data: {Markup.Escape(line)}[/]", settings);
                }
            }
        };

        try
        {
            var results = await networkService.StartTcpListener(
                settings.Port,
                tcpResultsFile,
                true,
                settings.ConnectionTimeout,
                settings.DataTimeout);

            WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

            if (File.Exists(tcpResultsFile))
            {
                var tcpResults = await File.ReadAllTextAsync(tcpResultsFile);
                WriteConsoleOutput($"  - Analyzing test results...", settings);
                WriteConsoleMarkup($"    Saved test results to: [green]{Markup.Escape(tcpResultsFile)}[/].", settings);

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

    protected string ExtractNumber(string text, string prefix)
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
