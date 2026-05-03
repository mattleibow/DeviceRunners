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

        [Description("Output format for test results (trx or txt)")]
        [CommandOption("--format")]
        [DefaultValue("trx")]
        public string Format { get; set; } = "trx";

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

        // Choose formatter and file extension based on --format
        var (formatter, extension) = settings.Format.ToLowerInvariant() switch
        {
            "txt" => ((IResultChannelFormatter)new TextResultChannelFormatter(), ".txt"),
            _ => (new TrxResultChannelFormatter(), ".trx"),
        };
        var resultsFile = Path.Combine(settings.ResultsDirectory, $"test-results{extension}");

        WriteConsoleOutput($"    Events file:  [green]{Markup.Escape(eventsFile)}[/]", settings);
        WriteConsoleOutput($"    Results file: [green]{Markup.Escape(resultsFile)}[/]", settings);

        WriteConsoleOutput($"  - Waiting for test events via TCP...", settings);
        WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

        var lastConnectTime = DateTimeOffset.UtcNow;
        var networkService = new NetworkService();

        // Use a FileResultChannel for the output file — it handles open/close, locking, flushing
        var resultChannel = new FileResultChannel(new FileResultChannelOptions
        {
            FilePath = resultsFile,
            Formatter = formatter,
        });

        int failedCount = 0;
        int totalCount = 0;

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
                ProcessLine(lineBuffer.ToString(), resultChannel, ref failedCount, ref totalCount, settings);
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
                        ProcessLine(trimmedLine, resultChannel, ref failedCount, ref totalCount, settings);
                    }
                }
            }
        };

        try
        {
            // Open the result channel before listening
            await resultChannel.OpenChannel();

            await networkService.StartTcpListener(
                settings.Port,
                eventsFile,
                true,
                settings.ConnectionTimeout,
                settings.DataTimeout);

            WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

            // Close the channel — this flushes and finalizes the output file
            await resultChannel.CloseChannel();

            WriteConsoleOutput($"  - Generated results file: [green]{Markup.Escape(resultsFile)}[/]", settings);

            // Report summary
            var passedCount = totalCount - failedCount;
            WriteConsoleOutput($"  - Results: Total={totalCount}, Passed={passedCount}, Failed={failedCount}", settings);

            if (totalCount == 0)
            {
                WriteConsoleOutput($"    [yellow]No test results received.[/]", settings);
                return (1, null);
            }

            return (failedCount, null);
        }
        catch (OperationCanceledException)
        {
            WriteConsoleOutput($"    [yellow]TCP listener timed out waiting for results.[/]", settings);

            // Close the channel to flush partial results
            await resultChannel.CloseChannel();

            if (totalCount > 0)
                return (failedCount > 0 ? failedCount : 1, null);

            return (1, null);
        }
    }

    void ProcessLine(string trimmedLine, IResultChannel resultChannel, ref int failedCount, ref int totalCount, TSettings settings)
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

        switch (evt.Type)
        {
            case TestResultEvent.TypeBegin:
                WriteConsoleOutput($"    [blue]Test run started: {Markup.Escape(evt.Message ?? "")}[/]", settings);
                break;

            case TestResultEvent.TypeResult:
                var resultInfo = evt.ToInfo();
                resultChannel.RecordResult(resultInfo);
                totalCount++;

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
                break;
        }
    }
}
