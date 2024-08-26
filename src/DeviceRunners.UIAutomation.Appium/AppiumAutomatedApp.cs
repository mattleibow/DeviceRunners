using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

/// <summary>
/// This type represents an automated app that is driven by Appium.
/// </summary>
public class AppiumAutomatedApp : IAutomatedApp
{
	private readonly AppiumAutomatedAppOptions _options;
	private readonly IAppiumDiagnosticLogger? _logger;
	private readonly AppiumDriverManager _driverManager;

	private bool _disposed;

	public AppiumAutomatedApp(AppiumAutomationFramework appium, AppiumAutomatedAppOptions options, IAppiumDiagnosticLogger? logger = null)
	{
		Framework = appium;
		_options = options;
		_logger = logger;
		_driverManager = new AppiumDriverManager(Framework.ServiceManager, _options);
		Commands = new AutomatedAppCommandExecutor(this);
	}

	public AppiumAutomationFramework Framework { get; }

	public AppiumServiceManager ServiceManager
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, typeof(AppiumAutomatedApp));
			return Framework.ServiceManager;
		}
	}

	public AppiumDriverManager DriverManager
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, typeof(AppiumAutomatedApp));
			return _driverManager;
		}
	}

	public AppiumDriver Driver => DriverManager.Driver;

	public IAutomatedAppCommandManager Commands { get; }

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		_driverManager.Dispose();
	}
}
