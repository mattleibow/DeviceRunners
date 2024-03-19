using OpenQA.Selenium.Appium;

namespace DeviceRunners.Appium;

public class AppiumDriverManager : IDisposable
{
	private readonly AppiumDriverManagerOptions _options;
	private readonly AppiumServiceManager _appium;
	private readonly IAppiumDiagnosticLogger? _logger;

	private AppiumDriver _driver = null!;

	public AppiumDriverManager(AppiumDriverManagerOptions options, AppiumServiceManager appium, IAppiumDiagnosticLogger? logger = null)
	{
		_options = options;
		_appium = appium;
		_logger = logger;
		
		StartDriver();
	}

	public AppiumDriver Driver => _driver;

	public void Restart()
	{
		ShutdownDriver();
		StartDriver();
	}

	public void Dispose()
	{
		ShutdownDriver();
	}

	private void ShutdownDriver()
	{
		_logger?.Log("Driver shutting down...");
		var ticks = Environment.TickCount;

		_driver.Dispose();

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - ticks).TotalSeconds;
		_logger?.Log($"Driver restarted in {delta} seconds.");
	}

	private void StartDriver()
	{
		_logger?.Log("Starting driver...");
		var ticks = Environment.TickCount;

		_driver = _options.DriverFactory.CreateDriver(_options, _appium);

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - ticks).TotalSeconds;
		_logger?.Log($"Driver started in {delta} seconds.");
	}
}
