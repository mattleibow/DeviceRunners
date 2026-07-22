using System.ComponentModel;

using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using DeviceRunners.VisualRunners;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WasmTestCommand(IAnsiConsole console) : BaseTestCommand<WasmTestCommand.Settings>(console)
{
	public class Settings : BaseTestCommandSettings
	{
		[Description("Run browser in headed mode (visible)")]
		[CommandOption("--headed")]
		public bool Headed { get; set; }

		[Description("HTTP port for the WASM web server (0 = auto)")]
		[CommandOption("--server-port")]
		[DefaultValue(0)]
		public int ServerPort { get; set; }
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		try
		{
			WriteConsoleOutput($"[blue]============================================================[/]", settings);
			WriteConsoleOutput($"[blue]WASM TEST RUN[/]", settings);
			WriteConsoleOutput($"[blue]============================================================[/]", settings);

			// Validate app path
			var appPath = Path.GetFullPath(settings.App);
			if (!Directory.Exists(appPath))
				throw new DirectoryNotFoundException($"WASM app directory not found: {appPath}");

			var indexPath = Path.Combine(appPath, "index.html");
			if (!File.Exists(indexPath))
				throw new FileNotFoundException($"index.html not found in WASM app directory: {appPath}");

			WriteConsoleOutput($"  - App path: [green]{Markup.Escape(appPath)}[/]", settings);

			// Ensure artifacts directory exists
			Directory.CreateDirectory(settings.ResultsDirectory);

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

			// Start web server
			WriteConsoleOutput($"  - Starting web server...", settings);
			await using var webServer = new WasmWebServerService();
			await webServer.StartAsync(appPath, settings.ServerPort);
			var url = webServer.BaseUrl!;
			WriteConsoleOutput($"    Serving at: [green]{Markup.Escape(url)}[/]", settings);

			// Set up event stream service
			var eventStream = new EventStreamService();
			var testRunEnded = new TaskCompletionSource();

			eventStream.TestRunStarted += (_, e) =>
				WriteConsoleOutput($"    [blue]Test run started: {Markup.Escape(e.Message ?? "")}[/]", settings);

			eventStream.TestResultRecorded += (_, e) =>
			{
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
			{
				WriteConsoleOutput($"    [blue]Test run ended[/]", settings);
				testRunEnded.TrySetResult();
			};

			eventStream.UnparseableLine += (_, e) =>
				WriteConsoleOutput($"    [yellow]Console: {Markup.Escape(e.Line)}[/]", settings);

			// Launch browser
			WriteConsoleOutput($"  - Launching headless Chrome via CDP...", settings);
			WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

			if (resultChannel is not null)
				await resultChannel.OpenChannel();

			// Set up console log capture (like logcat.txt for Android or ios-device-log.txt for iOS)
			var consoleLogPath = Path.Combine(settings.ResultsDirectory, "browser-console.log");
			await using var consoleLog = new StreamWriter(consoleLogPath, append: false) { AutoFlush = true };

			await using var browser = new BrowserService();

			// Two timeouts, mirroring the TCP runner so every platform behaves the
			// same way:
			//  - connection timeout: how long to wait for the FIRST browser message
			//    (a sign the app booted and started reporting);
			//  - data timeout: an inactivity timeout that resets on every message, so
			//    a long but healthy run keeps going as long as it produces output.
			// A single resettable source models the connection-then-inactivity window:
			// it starts with the connection timeout and, once any message arrives, is
			// reset to the (shorter) data timeout on every subsequent message. The
			// reset is guarded below because a late console message can race with the
			// disposal of the source during teardown.
			using var inactivityCts = new CancellationTokenSource();
			inactivityCts.CancelAfter(TimeSpan.FromSeconds(settings.ConnectionTimeout));
			inactivityCts.Token.Register(() => testRunEnded.TrySetCanceled());

			browser.ConsoleMessageReceived += (_, msg) =>
			{
				consoleLog.WriteLine(msg);
				// Reset the inactivity window on every message received. Guard against a
				// message racing with disposal at the very end of the run.
				try
				{
					if (!inactivityCts.IsCancellationRequested)
						inactivityCts.CancelAfter(TimeSpan.FromSeconds(settings.DataTimeout));
				}
				catch (ObjectDisposedException)
				{
				}
				eventStream.ReceiveData(msg + "\n");
			};

			var testUrl = $"{url}?device-runners-autorun=1";
			if (!string.IsNullOrWhiteSpace(settings.Filter))
				testUrl += $"&device-runners-filter={Uri.EscapeDataString(settings.Filter)}";
			await browser.LaunchAsync(testUrl, headless: !settings.Headed);
			WriteConsoleOutput($"    Browser launched, running tests...", settings);

			// Wait for test completion or one of the timeouts.
			try
			{
				await testRunEnded.Task;
			}
			catch (OperationCanceledException)
			{
				WriteConsoleOutput($"    [yellow]Browser timed out waiting for test results.[/]", settings);
			}

			eventStream.Flush();

			// Classify the run the same way the TCP listener does so a crash or
			// timeout mid-run (begin + some results, but no "end") is reported as a
			// failure instead of being mistaken for a successful run. A clean empty
			// run (begin + end, no results) still succeeds, mirroring `dotnet test
			// --filter` exit 0.
			var outcome = ClassifyRun(eventStream.HasStarted, eventStream.HasEnded, eventStream.TotalCount);

			WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

			if (resultChannel is not null)
				await resultChannel.CloseChannel();

			if (resultsFile is not null)
				WriteConsoleOutput($"  - Generated results file: [green]{Markup.Escape(resultsFile)}[/]", settings);

			WriteConsoleOutput($"  - Browser console log: [green]{Markup.Escape(consoleLogPath)}[/]", settings);

			WriteConsoleOutput($"  - Results: Total={eventStream.TotalCount}, Passed={eventStream.PassedCount}, Failed={eventStream.FailedCount}, Skipped={eventStream.SkippedCount}", settings);

			if (outcome == TestRunOutcome.Crashed)
				WriteConsoleOutput($"    [red]The app crashed or timed out before the run completed. Only {eventStream.TotalCount} test result(s) were received before the connection was lost. Check browser-console.log for details.[/]", settings);
			else if (outcome == TestRunOutcome.NoResults)
				WriteConsoleOutput($"    [red]No test results received. This usually means the browser failed to navigate, the app didn't boot, or the autorun query parameter was not detected. Check browser-console.log for details.[/]", settings);
			else if (outcome == TestRunOutcome.CleanEmpty)
				WriteConsoleOutput($"    [yellow]No test matches the given test filter. The run completed with no results.[/]", settings);

			var errorMessage = outcome switch
			{
				TestRunOutcome.Crashed => "The app crashed or timed out mid-run — check browser-console.log",
				TestRunOutcome.NoResults => "No test results received — check browser-console.log",
				_ => (string?)null,
			};

			var result = new TestStartResult
			{
				Success = OutcomeIsSuccess(outcome, eventStream.FailedCount),
				AppPath = settings.App,
				ResultsDirectory = settings.ResultsDirectory,
				TestFailures = eventStream.FailedCount,
				TestResults = resultsFile,
				ErrorMessage = errorMessage
			};
			WriteResult(result, settings);

			return OutcomeToExitCode(outcome, eventStream.FailedCount);
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
}
