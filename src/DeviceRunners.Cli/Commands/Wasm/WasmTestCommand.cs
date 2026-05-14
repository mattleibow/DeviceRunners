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

		[Description("Test execution timeout in seconds")]
		[CommandOption("--timeout")]
		[DefaultValue(300)]
		public int Timeout { get; set; } = 300;
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

			browser.ConsoleMessageReceived += (_, msg) =>
			{
				consoleLog.WriteLine(msg);
				eventStream.ReceiveData(msg + "\n");
			};

			var testUrl = $"{url}?device-runners-autorun=1";
			await browser.LaunchAsync(testUrl, headless: !settings.Headed);
			WriteConsoleOutput($"    Browser launched, running tests...", settings);

			// Wait for test completion or timeout
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(settings.Timeout));
			cts.Token.Register(() => testRunEnded.TrySetCanceled());

			try
			{
				await testRunEnded.Task;
			}
			catch (TaskCanceledException)
			{
				WriteConsoleOutput($"    [yellow]Test execution timed out after {settings.Timeout}s[/]", settings);
			}

			eventStream.Flush();

			WriteConsoleOutput($"[blue]------------------------------------------------------------[/]", settings);

			if (resultChannel is not null)
				await resultChannel.CloseChannel();

			if (resultsFile is not null)
				WriteConsoleOutput($"  - Generated results file: [green]{Markup.Escape(resultsFile)}[/]", settings);

			WriteConsoleOutput($"  - Browser console log: [green]{Markup.Escape(consoleLogPath)}[/]", settings);

			WriteConsoleOutput($"  - Results: Total={eventStream.TotalCount}, Passed={eventStream.PassedCount}, Failed={eventStream.FailedCount}, Skipped={eventStream.SkippedCount}", settings);

			if (eventStream.TotalCount == 0)
				WriteConsoleOutput($"    [yellow]No test results received.[/]", settings);

			var result = new TestStartResult
			{
				Success = eventStream.FailedCount == 0 && eventStream.TotalCount > 0,
				AppPath = settings.App,
				ResultsDirectory = settings.ResultsDirectory,
				TestFailures = eventStream.FailedCount,
				TestResults = resultsFile
			};
			WriteResult(result, settings);

			return eventStream.FailedCount > 0 || eventStream.TotalCount == 0 ? 1 : 0;
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
