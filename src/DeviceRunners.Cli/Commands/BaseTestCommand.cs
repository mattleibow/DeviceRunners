using System.ComponentModel;

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

        [Description("Logger for test results (trx or txt). If not specified, no results file is produced.")]
        [CommandOption("--logger")]
        public string? Logger { get; set; }

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

        // Set up result channel only if --logger is specified
        IResultChannel? resultChannel = null;
        string? resultsFile = null;

        if (settings.Logger is not null)
        {
            var (loggerName, logFileName) = ParseLogger(settings.Logger);

            var (formatter, extension) = loggerName switch
            {
                "txt" => ((IResultChannelFormatter)new TextResultChannelFormatter(), ".txt"),
                "trx" => (new TrxResultChannelFormatter(), ".trx"),
                _ => throw new InvalidOperationException($"Unknown logger '{loggerName}'. Supported values: trx, txt"),
            };

            resultsFile = logFileName is not null
                ? Path.Combine(settings.ResultsDirectory, logFileName)
                : Path.Combine(settings.ResultsDirectory, $"test-results{extension}");

            resultChannel = new FileResultChannel(new FileResultChannelOptions
            {
                FilePath = resultsFile,
                Formatter = formatter,
            });

            WriteConsoleOutput($"    Results file: [green]{Markup.Escape(resultsFile)}[/]", settings);
        }

        WriteConsoleOutput($"    Events file:  [green]{Markup.Escape(eventsFile)}[/]", settings);
        WriteConsoleOutput($"  - Waiting for test events via TCP...", settings);
        WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

        // Set up event stream service (pure parser + event emitter)
        var eventStream = new EventStreamService();
        var networkService = new NetworkService();

        // Wire up event stream events for console output
        var lastConnectTime = DateTimeOffset.UtcNow;

        eventStream.TestRunStarted += (_, e) =>
            WriteConsoleOutput($"    [blue]Test run started: {Markup.Escape(e.Message ?? "")}[/]", settings);

        eventStream.TestResultRecorded += (_, e) =>
        {
            // Forward to result channel if configured
            resultChannel?.RecordResult(e.Result);

            var statusColor = e.Result.Status switch
            {
                TestResultStatus.Passed => "green",
                TestResultStatus.Failed => "red",
                TestResultStatus.Skipped => "yellow",
                _ => "white",
            };
            WriteConsoleOutput($"    [{statusColor}]{Markup.Escape(e.Event.DisplayName ?? "?")} - {e.Event.Status}[/]", settings);
        };

        eventStream.TestRunEnded += (_, _) =>
            WriteConsoleOutput($"    [blue]Test run ended[/]", settings);

        eventStream.UnparseableLine += (_, e) =>
            WriteConsoleOutput($"    [yellow]Unparseable: {Markup.Escape(e.Line)}[/]", settings);

        // Wire up network events
        networkService.ConnectionEstablished += (_, e) =>
        {
            var delta = e.Timestamp - lastConnectTime;
            lastConnectTime = DateTimeOffset.UtcNow;
            WriteConsoleOutput($"    [yellow]TCP connection established with {e.RemoteEndPoint} after {delta}[/]", settings);
        };

        networkService.ConnectionClosed += (_, e) =>
        {
            eventStream.Flush();
            lastConnectTime = DateTimeOffset.UtcNow;
            WriteConsoleOutput($"    [yellow]TCP connection closed with {e.RemoteEndPoint}[/]", settings);
        };

        networkService.DataReceived += (_, e) => eventStream.ReceiveData(e.Data);

        try
        {
            if (resultChannel is not null)
                await resultChannel.OpenChannel();

            await networkService.StartTcpListener(
                settings.Port,
                eventsFile,
                true,
                settings.ConnectionTimeout,
                settings.DataTimeout);

            WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);
        }
        catch (OperationCanceledException)
        {
            WriteConsoleOutput($"    [yellow]TCP listener timed out waiting for results.[/]", settings);
        }
        finally
        {
            if (resultChannel is not null)
                await resultChannel.CloseChannel();
        }

        if (resultsFile is not null)
            WriteConsoleOutput($"  - Generated results file: [green]{Markup.Escape(resultsFile)}[/]", settings);

        WriteConsoleOutput($"  - Results: Total={eventStream.TotalCount}, Passed={eventStream.PassedCount}, Failed={eventStream.FailedCount}, Skipped={eventStream.SkippedCount}", settings);

        // Detect app crash: if we received a "begin" event and test results but
        // never got the "end" event, the app crashed or was killed mid-run.
        // Return -1 to signal crash to the caller (mapped to exit code 2).
        if (eventStream.HasStarted && !eventStream.HasEnded && eventStream.TotalCount > 0)
        {
            WriteConsoleOutput($"    [red]The application appears to have crashed during the test run.[/]", settings);
            WriteConsoleOutput($"    [red]Only {eventStream.TotalCount} test result(s) were received before the connection was lost.[/]", settings);
            WriteConsoleOutput($"    [red]Check the device log for crash details.[/]", settings);
            return (-1, resultsFile);
        }

        if (eventStream.TotalCount == 0)
        {
            WriteConsoleOutput($"    [yellow]No test results received.[/]", settings);
            return (1, null);
        }

        return (eventStream.FailedCount, resultsFile);
    }

    /// <summary>
    /// Parses a logger string in the format "name" or "name;LogFileName=file.ext"
    /// matching dotnet test --logger conventions.
    /// </summary>
    internal static (string name, string? logFileName) ParseLogger(string logger)
    {
        var semicolonIndex = logger.IndexOf(';');
        if (semicolonIndex < 0)
            return (logger.ToLowerInvariant(), null);

        var name = logger[..semicolonIndex].ToLowerInvariant();
        var parameters = logger[(semicolonIndex + 1)..];

        string? logFileName = null;
        foreach (var param in parameters.Split(';'))
        {
            var eqIndex = param.IndexOf('=');
            if (eqIndex < 0)
                continue;

            var key = param[..eqIndex].Trim();
            var value = param[(eqIndex + 1)..].Trim();

            if (key.Equals("LogFileName", StringComparison.OrdinalIgnoreCase))
                logFileName = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return (name, logFileName);
    }
}
