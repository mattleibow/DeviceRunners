using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class AppiumDriverManager : IDisposable
{
	private readonly AppiumAutomatedAppOptions _options;
	private readonly AppiumServiceManager _appium;
	private readonly IAppiumDiagnosticLogger? _logger;

	private AppiumDriver _driver = null!;

	public AppiumDriverManager(AppiumServiceManager appium, AppiumAutomatedAppOptions options, IAppiumDiagnosticLogger? logger = null)
	{
		_appium = appium;
		_options = options;
		_logger = logger;
	}

	public AppiumDriver Driver => _driver;

	public void StartDriver()
	{
		_logger?.Log("Starting driver...");
		var ticks = Environment.TickCount;

		_driver = _options.DriverFactory.CreateDriver(_appium, _options);

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - ticks).TotalSeconds;
		_logger?.Log($"Driver started in {delta} seconds.");
	}

	public void ShutdownDriver()
	{
		_logger?.Log("Driver shutting down...");
		var ticks = Environment.TickCount;

		_driver.Dispose();

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
