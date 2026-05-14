using System.ComponentModel;

using DeviceRunners.Cli.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WasmServeCommand(IAnsiConsole console) : BaseAsyncCommand<WasmServeCommand.Settings>(console)
{
	public class Settings : BaseAsyncCommandSettings
	{
		[Description("Path to the published WASM app directory")]
		[CommandOption("--app")]
		public required string App { get; set; }

		[Description("HTTP port for the web server")]
		[CommandOption("--port")]
		[DefaultValue(5000)]
		public int Port { get; set; } = 5000;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var appPath = Path.GetFullPath(settings.App);
		if (!Directory.Exists(appPath))
		{
			WriteConsoleOutput($"[red]Directory not found: {Markup.Escape(appPath)}[/]", settings);
			return 1;
		}

		WriteConsoleOutput($"[green]Starting WASM web server...[/]", settings);
		WriteConsoleOutput($"  App path: [blue]{Markup.Escape(appPath)}[/]", settings);

		await using var webServer = new WasmWebServerService();
		await webServer.StartAsync(appPath, settings.Port);

		WriteConsoleOutput($"  Serving at: [blue]{Markup.Escape(webServer.BaseUrl!)}[/]", settings);
		WriteConsoleOutput($"  Press Ctrl+C to stop.", settings);

		using var cts = new CancellationTokenSource();
		ConsoleCancelEventHandler cancelHandler = (_, e) =>
		{
			e.Cancel = true;
			cts.Cancel();
		};
		Console.CancelKeyPress += cancelHandler;

		try
		{
			await Task.Delay(Timeout.Infinite, cts.Token);
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			Console.CancelKeyPress -= cancelHandler;
		}

		WriteConsoleOutput($"[green]Server stopped.[/]", settings);
		return 0;
	}
}
