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

		[Description("TCP port the app should connect back to (defaults to --port)")]
		[CommandOption("--app-port")]
		public int? AppPort { get; set; }

		[Description("Semicolon-separated host names the app should try connecting to (defaults to localhost)")]
		[CommandOption("--app-host-names")]
		public string? AppHostNames { get; set; }

		[Description("Connection timeout in seconds")]
		[CommandOption("--connection-timeout")]
		[DefaultValue(120)]
		public int ConnectionTimeout { get; set; } = 120;

		[Description("Data timeout in seconds")]
		[CommandOption("--data-timeout")]
		[DefaultValue(30)]
		public int DataTimeout { get; set; } = 30;

		[Description("Run only the tests matching the given dotnet test --filter style expression")]
		[CommandOption("--filter")]
		public string? Filter { get; set; }
	}

	protected override int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
	{
		return ExecuteAsync(context, settings).GetAwaiter().GetResult();
	}

	protected abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings);

	/// <summary>
	/// Builds the environment variables that tell the test app how to connect
	/// back to the CLI's TCP listener.
	/// </summary>
	internal static Dictionary<string, string> GetAppEnvironmentVariables(TSettings settings)
	{
		var variables = new Dictionary<string, string>
		{
			["DEVICE_RUNNERS_AUTORUN"] = "1",
			["DEVICE_RUNNERS_PORT"] = (settings.AppPort ?? settings.Port).ToString(),
			["DEVICE_RUNNERS_HOST_NAMES"] = settings.AppHostNames ?? "localhost",
		};

		if (!string.IsNullOrWhiteSpace(settings.Filter))
			variables["DEVICE_RUNNERS_FILTER"] = settings.Filter!;

		return variables;
	}

	protected record TestListenerResult(int FailedCount, string? ResultsFile, bool Crashed)
	{
		// Exit codes: 0 = success, 1 = test failures, 2 = app crashed
		public int ToExitCode() => Crashed ? 2 : FailedCount > 0 ? 1 : 0;

		public bool Success => FailedCount == 0 && !Crashed;
	}

	protected async Task<TestListenerResult> StartTestListener(TSettings settings)
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

		// Detect app crash or timeout: if we received a "begin" event and test
		// results but never got the "end" event, the app crashed, was killed, or
		// timed out mid-run. Signal that as a crash (mapped to exit code 2) so an
		// incomplete run never looks like a clean pass.
		var outcome = ClassifyRun(eventStream.HasStarted, eventStream.HasEnded, eventStream.TotalCount);

		if (outcome == TestRunOutcome.Crashed)
		{
			WriteConsoleOutput($"    [red]The application appears to have crashed during the test run.[/]", settings);
			WriteConsoleOutput($"    [red]Only {eventStream.TotalCount} test result(s) were received before the connection was lost.[/]", settings);
			WriteConsoleOutput($"    [red]Check the device log for crash details.[/]", settings);
			return new TestListenerResult(eventStream.FailedCount, resultsFile, Crashed: true);
		}

		// A clean empty run (begin + end received, but no test results) is a success,
		// not a failure — it mirrors `dotnet test --filter`, which exits 0 with
		// "No test matches the given testcase filter". Only treat a missing connection
		// (no "begin" event at all) as a failure.
		if (outcome == TestRunOutcome.CleanEmpty)
		{
			WriteConsoleOutput($"    [yellow]No test matches the given test filter. The run completed with no results.[/]", settings);
			return new TestListenerResult(0, resultsFile, Crashed: false);
		}

		if (outcome == TestRunOutcome.NoResults)
		{
			WriteConsoleOutput($"    [yellow]No test results received.[/]", settings);
			return new TestListenerResult(1, null, Crashed: false);
		}

		return new TestListenerResult(eventStream.FailedCount, resultsFile, Crashed: false);
	}

	internal enum TestRunOutcome
	{
		/// <summary>Begin, results, and end were all received — a normal run.</summary>
		Completed,

		/// <summary>Begin and end received with zero results — a successful empty run (e.g. a zero-match filter).</summary>
		CleanEmpty,

		/// <summary>Begin and results received but no end — the app crashed mid-run.</summary>
		Crashed,

		/// <summary>No connection or no begin event — the app never reported a run.</summary>
		NoResults,
	}

	/// <summary>
	/// Classifies a test run from the event-stream flags, distinguishing a clean
	/// empty run (zero-match filter, still a success) from a crash or a missing
	/// connection.
	/// </summary>
	internal static TestRunOutcome ClassifyRun(bool hasStarted, bool hasEnded, int totalCount)
	{
		if (hasStarted && !hasEnded && totalCount > 0)
			return TestRunOutcome.Crashed;

		if (hasStarted && hasEnded && totalCount == 0)
			return TestRunOutcome.CleanEmpty;

		if (totalCount == 0)
			return TestRunOutcome.NoResults;

		return TestRunOutcome.Completed;
	}

	/// <summary>
	/// Whether the given outcome represents a successful run: a normal completion
	/// with no failures, or a clean zero-match run. A crash or a missing run is
	/// never a success, even if no individual failures were recorded.
	/// </summary>
	internal static bool OutcomeIsSuccess(TestRunOutcome outcome, int failedCount) =>
		outcome switch
		{
			TestRunOutcome.Completed => failedCount == 0,
			TestRunOutcome.CleanEmpty => true,
			_ => false,
		};

	/// <summary>
	/// Maps an outcome to a process exit code, matching the TCP listener semantics:
	/// 0 = success or clean empty run, 1 = test failures or no results, 2 = crash.
	/// </summary>
	internal static int OutcomeToExitCode(TestRunOutcome outcome, int failedCount) =>
		outcome switch
		{
			TestRunOutcome.Crashed => 2,
			TestRunOutcome.NoResults => 1,
			TestRunOutcome.CleanEmpty => 0,
			_ => failedCount > 0 ? 1 : 0,
		};

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
