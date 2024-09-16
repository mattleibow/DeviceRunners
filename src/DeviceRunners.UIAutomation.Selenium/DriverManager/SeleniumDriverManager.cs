using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumDriverManager : IDisposable
{
	private readonly SeleniumAutomatedAppOptions _options;
	private readonly IDiagnosticLogger? _logger;

	private WebDriver? _driver;

	public SeleniumDriverManager(SeleniumAutomatedAppOptions options, IDiagnosticLogger? logger = null)
	{
		_options = options;
		_logger = logger;
	}

	public WebDriver? Driver => _driver;

	public bool IsRunning => Driver?.SessionId is not null;

	public void StartDriver()
	{
		_logger?.Log("Starting driver...");

		var ticks = Environment.TickCount;
		_driver = _options.DriverFactory.CreateDriver(_options);
		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - ticks).TotalSeconds;

		_logger?.Log($"Driver started in {delta} seconds.");

		if (_options.DriverOptions.TryGetInitialUrl(out var initialUrl))
		{
			_logger?.Log($"Navigating to initial URL '{initialUrl}'...");

			var ticksNavigate = Environment.TickCount;
			_driver.Navigate().GoToUrl(initialUrl);
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

		_driver?.Dispose();

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
