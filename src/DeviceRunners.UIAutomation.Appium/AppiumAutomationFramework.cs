namespace DeviceRunners.UIAutomation.Appium;

/// <summary>
/// This type is responsible for creating and managing Appium automated app instances.
/// </summary>
public class AppiumAutomationFramework : IAutomationFramework
{
	private readonly AppiumServiceManagerOptions _options;
	private readonly IReadOnlyList<AppiumAutomatedAppOptions> _apps;
	private readonly IDiagnosticLogger? _logger;
	private AppiumServiceManager? _serviceManager;
	private bool _disposed;

	public AppiumAutomationFramework(AppiumServiceManagerOptions options, IEnumerable<AppiumAutomatedAppOptions> apps, IDiagnosticLogger? logger = null)
	{
		_options = options;
		_apps = apps.ToList();
		_logger = logger;
	}

	public AppiumServiceManager ServiceManager
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, typeof(AppiumAutomationFramework));
			return _serviceManager ??= new AppiumServiceManager(_options, _logger);
		}
	}

	public IReadOnlyList<AppiumAutomatedAppOptions> AvailableApps => _apps;

	public IAutomatedApp CreateApp(AppiumAutomatedAppOptions options)
	{
		return new AppiumAutomatedApp(this, options, _logger);
	}

	public void StartApp(AppiumAutomatedApp app)
	{
		app.DriverManager.StartDriver();
	}

	public void StopApp(AppiumAutomatedApp app)
	{
		app.DriverManager.ShutdownDriver();
	}

	public void RestartApp(AppiumAutomatedApp app)
	{
		app.DriverManager.RestartDriver();
	}

	IReadOnlyList<IAutomatedAppOptions> IAutomationFramework.AvailableApps => _apps;

	IAutomatedApp IAutomationFramework.CreateApp(IAutomatedAppOptions options)
	{
		if (options is not AppiumAutomatedAppOptions appiumOptions)
			throw new ArgumentException($"Expected {nameof(AppiumAutomatedAppOptions)} but got {options.GetType().Name}", nameof(options));

		return CreateApp(appiumOptions);
	}

	void IAutomationFramework.StartApp(IAutomatedApp app) => StartApp(AsAppiumApp(app));

	void IAutomationFramework.StopApp(IAutomatedApp app) => StopApp(AsAppiumApp(app));

	void IAutomationFramework.RestartApp(IAutomatedApp app) => RestartApp(AsAppiumApp(app));

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		_serviceManager?.Dispose();
	}

	private static AppiumAutomatedApp AsAppiumApp(IAutomatedApp app)
	{
		if (app is not AppiumAutomatedApp appiumApp)
			throw new ArgumentException($"Expected {nameof(AppiumAutomatedApp)} but got {app.GetType().Name}", nameof(app));
		return appiumApp;
	}
}
