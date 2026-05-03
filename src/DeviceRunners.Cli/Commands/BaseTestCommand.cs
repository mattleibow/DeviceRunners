using System.ComponentModel;
using System.Text;

using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using DeviceRunners.VisualRunners;

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
        [DefaultValue(120)]
        public int ConnectionTimeout { get; set; } = 120;

        [Description("Data timeout in seconds")]
        [CommandOption("--data-timeout")]
        [DefaultValue(30)]
        public int DataTimeout { get; set; } = 30;
    }

    public override int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    protected abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings);

    protected async Task<(int testFailures, string? testResults)> StartTestListener(TSettings settings)
    {
        // Ensure artifacts directory exists
        Directory.CreateDirectory(settings.ResultsDirectory);

        WriteConsoleOutput($"  - Starting TCP listener on port {settings.Port}...", settings);

        var eventsFile = Path.Combine(settings.ResultsDirectory, "tcp-test-events.jsonl");
        var trxFile = Path.Combine(settings.ResultsDirectory, "test-results.trx");

        WriteConsoleOutput($"    Events file: [green]{Markup.Escape(eventsFile)}[/]", settings);
        WriteConsoleOutput($"    TRX file:    [green]{Markup.Escape(trxFile)}[/]", settings);

        WriteConsoleOutput($"  - Waiting for test events via TCP...", settings);
        WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

        var lastConnectTime = DateTimeOffset.UtcNow;
        var networkService = new NetworkService();

        // Collect all events for processing
        List<string> eventLines = [];
        List<ITestResultInfo> testResults = [];
        int failedCount = 0;

        // CLI-side text formatter for live console output
        var textFormatter = new TextResultChannelFormatter();
        var consoleWriter = new StringWriter();

        // Line buffer for reassembling NDJSON lines split across TCP chunks
        var lineBuffer = new StringBuilder();

        networkService.ConnectionEstablished += (sender, e) =>
        {
            var delta = e.Timestamp - lastConnectTime;
            lastConnectTime = DateTimeOffset.UtcNow;
            WriteConsoleOutput($"    [yellow]TCP connection established with {e.RemoteEndPoint} after {delta}[/]", settings);
        };

        networkService.ConnectionClosed += (sender, e) =>
        {
            // Flush any remaining buffered data as a final line
            if (lineBuffer.Length > 0)
            {
                ProcessLine(lineBuffer.ToString(), eventLines, testResults, ref failedCount, textFormatter, consoleWriter, settings);
                lineBuffer.Clear();
            }

            lastConnectTime = DateTimeOffset.UtcNow;
            WriteConsoleOutput($"    [yellow]TCP connection closed with {e.RemoteEndPoint}[/]", settings);
        };

        networkService.DataReceived += (sender, e) =>
        {
            // Accumulate data and process only complete lines
            lineBuffer.Append(e.Data);

            // Extract complete lines (terminated by '\n')
            var buffered = lineBuffer.ToString();
            var lastNewline = buffered.LastIndexOf('\n');
            if (lastNewline >= 0)
            {
                // Process all complete lines
                var completeData = buffered[..lastNewline];
                lineBuffer.Clear();
                lineBuffer.Append(buffered[(lastNewline + 1)..]);

                foreach (var line in completeData.Split('\n'))
                {
                    var trimmedLine = line.TrimEnd('\r');
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        ProcessLine(trimmedLine, eventLines, testResults, ref failedCount, textFormatter, consoleWriter, settings);
                    }
                }
            }
        };

        try
        {
            await networkService.StartTcpListener(
                settings.Port,
                null, // We handle file output ourselves
                true,
                settings.ConnectionTimeout,
                settings.DataTimeout);

            WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

            // Save raw NDJSON events
            if (eventLines.Count > 0)
            {
                await File.WriteAllLinesAsync(eventsFile, eventLines);
                WriteConsoleOutput($"  - Saved {eventLines.Count} events to: [green]{Markup.Escape(eventsFile)}[/]", settings);
            }

            // Generate TRX file from collected results
            if (testResults.Count > 0)
            {
                WriteTrxFile(trxFile, testResults);
                WriteConsoleOutput($"  - Generated TRX file: [green]{Markup.Escape(trxFile)}[/]", settings);
            }

            // Report summary
            var totalTests = testResults.Count;
            var passedCount = testResults.Count(r => r.Status == TestResultStatus.Passed);
            var skippedCount = testResults.Count(r => r.Status == TestResultStatus.Skipped);

            WriteConsoleOutput($"  - Results: Total={totalTests}, Passed={passedCount}, Failed={failedCount}, Skipped={skippedCount}", settings);

            if (totalTests == 0)
            {
                WriteConsoleOutput($"    [yellow]No test results received.[/]", settings);
                return (1, null);
            }

            var allText = consoleWriter.ToString();
            return (failedCount, allText);
        }
        catch (OperationCanceledException)
        {
            WriteConsoleOutput($"    [yellow]TCP listener timed out waiting for results.[/]", settings);

            // Still save what we got
            if (eventLines.Count > 0)
            {
                await File.WriteAllLinesAsync(eventsFile, eventLines);

                // Generate TRX from partial results
                if (testResults.Count > 0)
                    WriteTrxFile(trxFile, testResults);

                return (failedCount > 0 ? failedCount : 1, consoleWriter.ToString());
            }

            return (1, null);
        }
    }

    void ProcessLine(string trimmedLine, List<string> eventLines, List<ITestResultInfo> testResults, ref int failedCount, TextResultChannelFormatter textFormatter, StringWriter consoleWriter, TSettings settings)
    {
        // Skip ping/probe messages from TcpResultChannel host selection
        if (trimmedLine == "ping")
            return;

        var evt = TestResultEvent.Parse(trimmedLine);
        if (evt is null)
        {
            WriteConsoleOutput($"    [yellow]Unparseable: {Markup.Escape(trimmedLine)}[/]", settings);
            return;
        }

        // Only add successfully parsed events to the events file
        eventLines.Add(trimmedLine);

        switch (evt.Type)
        {
            case TestResultEvent.TypeBegin:
                WriteConsoleOutput($"    [blue]Test run started: {Markup.Escape(evt.Message ?? "")}[/]", settings);
                textFormatter.BeginTestRun(consoleWriter, evt.Message);
                break;

            case TestResultEvent.TypeResult:
                var resultInfo = evt.ToInfo();
                testResults.Add(resultInfo);
                textFormatter.RecordResult(resultInfo);

                var statusColor = resultInfo.Status switch
                {
                    TestResultStatus.Passed => "green",
                    TestResultStatus.Failed => "red",
                    TestResultStatus.Skipped => "yellow",
                    _ => "white",
                };
                WriteConsoleOutput($"    [{statusColor}]{Markup.Escape(evt.DisplayName ?? "?")} - {evt.Status}[/]", settings);

                if (resultInfo.Status == TestResultStatus.Failed)
                    failedCount++;
                break;

            case TestResultEvent.TypeEnd:
                WriteConsoleOutput($"    [blue]Test run ended[/]", settings);
                textFormatter.EndTestRun();
                break;
        }
    }

    static void WriteTrxFile(string trxFile, List<ITestResultInfo> testResults)
    {
        using var trxWriter = new StreamWriter(trxFile);
        var trxFormatter = new TrxResultChannelFormatter();
        trxFormatter.BeginTestRun(trxWriter);
        foreach (var result in testResults)
            trxFormatter.RecordResult(result);
        trxFormatter.EndTestRun();
    }
}
