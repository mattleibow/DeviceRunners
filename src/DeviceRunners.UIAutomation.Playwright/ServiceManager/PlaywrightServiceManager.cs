using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

/// <summary>
/// This type is responsible for managing the Playwright server lifecycle.
/// </summary>
public class PlaywrightServiceManager : IDisposable
{
	private readonly PlaywrightServiceManagerOptions _options;
	private readonly IDiagnosticLogger? _logger;
	private readonly IPlaywright _playwrightService;

	public PlaywrightServiceManager(PlaywrightServiceManagerOptions options, IDiagnosticLogger? logger = null)
	{
		_options = options;
		_logger = logger;
		_playwrightService = CreatePlaywrightServer();
	}

	public IPlaywright Service => _playwrightService;

	public bool IsRunning => _playwrightService is not null;

	private IPlaywright CreatePlaywrightServer()
	{
		_logger?.Log("Starting Playwright server...");

		var startTicks = Environment.TickCount;

		var creator = Microsoft.Playwright.Playwright.CreateAsync();
		var playwright = creator.GetAwaiter().GetResult();

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - startTicks).TotalSeconds;

		_logger?.Log($"Playwright server started in {delta} seconds.");

		return playwright;
	}

	public void Dispose()
	{
		_playwrightService.Dispose();
	}
}
