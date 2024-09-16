using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightDriverManager : IDisposable
{
	private readonly PlaywrightAutomatedAppOptions _options;
	private readonly PlaywrightServiceManager _playwright;
	private readonly IDiagnosticLogger? _logger;

	private IBrowser? _browser;
	private IPage? _page;

	public PlaywrightDriverManager(PlaywrightServiceManager playwright, PlaywrightAutomatedAppOptions options, IDiagnosticLogger? logger = null)
	{
		_playwright = playwright;
		_options = options;
		_logger = logger;
	}

	public IBrowser? Browser => _browser;

	public IPage? Page => _page;

	public bool IsRunning => Browser?.IsConnected ?? false && Page is not null;

	public void StartDriver()
	{
		_logger?.Log("Starting driver...");
		var ticks = Environment.TickCount;

		_browser = _options.DriverFactory.CreateDriver(_playwright, _options);
		_page = _browser.NewPageAsync().GetAwaiter().GetResult();

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - ticks).TotalSeconds;
		_logger?.Log($"Driver started in {delta} seconds.");

		if (_options.LaunchOptions.TryGetInitialUrl(out var initialUrl))
		{
			_logger?.Log($"Navigating to initial URL '{initialUrl}'...");

			var ticksNavigate = Environment.TickCount;
			_page.GotoAsync(initialUrl).GetAwaiter().GetResult();
			var deltaNavigate = TimeSpan.FromMilliseconds(Environment.TickCount - ticksNavigate).TotalSeconds;

			_logger?.Log($"Navigation completed in {deltaNavigate} seconds.");
		}
		else
		{
			_logger?.Log($"No initial URL was specified.");
		}
	}

	public void ShutdownDriver()
	{
		_logger?.Log("Driver shutting down...");
		var ticks = Environment.TickCount;

		_browser?.DisposeAsync().GetAwaiter().GetResult();

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - ticks).TotalSeconds;
		_logger?.Log($"Driver shut down in {delta} seconds.");
	}

	public void RestartDriver()
	{
		ShutdownDriver();
		StartDriver();
	}

	public void Dispose()
	{
		ShutdownDriver();
	}
}
