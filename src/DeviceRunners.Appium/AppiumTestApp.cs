using OpenQA.Selenium.Appium;

namespace DeviceRunners.Appium;

public class AppiumTestApp : IDisposable
{
	private readonly AppiumDriverManager _driverManager;
	private readonly AppiumTest _appiumTest;

	bool disposed;

	public AppiumTestApp(AppiumTest appiumTest, AppiumDriverManagerOptions options, IAppiumDiagnosticLogger logger)
	{
		_appiumTest = appiumTest;
		_driverManager = new AppiumDriverManager(options, appiumTest.ServiceManager);
	}

	public AppiumServiceManager ServiceManager => _appiumTest.ServiceManager;

	public AppiumDriverManager DriverManager
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, typeof(AppiumTest));

			return _driverManager;
		}
	}

	public AppiumDriver Driver => DriverManager.Driver;

	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		_driverManager.Dispose();
	}
}
