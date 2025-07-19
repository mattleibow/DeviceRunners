namespace DeviceRunners.UIAutomation.Selenium;

/// <summary>
/// This type is responsible for creating and managing Selenium automated app instances.
/// </summary>
public class SeleniumAutomationFramework : IAutomationFramework
{
	private readonly IReadOnlyList<SeleniumAutomatedAppOptions> _apps;
	private readonly IDiagnosticLogger? _logger;
	private bool _disposed;

	public SeleniumAutomationFramework(IEnumerable<SeleniumAutomatedAppOptions> apps, IDiagnosticLogger? logger = null)
	{
		_apps = apps.ToList();
		_logger = logger;
	}

	public IReadOnlyList<SeleniumAutomatedAppOptions> AvailableApps => _apps;

	public IAutomatedApp CreateApp(SeleniumAutomatedAppOptions options)
	{
		return new SeleniumAutomatedApp(this, options, _logger);
	}

	public void StartApp(SeleniumAutomatedApp app)
	{
		app.DriverManager.StartDriver();
	}

	public void StopApp(SeleniumAutomatedApp app)
	{
		app.DriverManager.ShutdownDriver();
	}

	public void RestartApp(SeleniumAutomatedApp app)
	{
		app.DriverManager.RestartDriver();
	}

	IReadOnlyList<IAutomatedAppOptions> IAutomationFramework.AvailableApps => _apps;

	IAutomatedApp IAutomationFramework.CreateApp(IAutomatedAppOptions options)
	{
		if (options is not SeleniumAutomatedAppOptions seleniumOptions)
			throw new ArgumentException($"Expected {nameof(SeleniumAutomatedAppOptions)} but got {options.GetType().Name}", nameof(options));

		return CreateApp(seleniumOptions);
	}

	void IAutomationFramework.StartApp(IAutomatedApp app) => StartApp(AsSeleniumApp(app));

	void IAutomationFramework.StopApp(IAutomatedApp app) => StopApp(AsSeleniumApp(app));

	void IAutomationFramework.RestartApp(IAutomatedApp app) => RestartApp(AsSeleniumApp(app));

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
	}

	private static SeleniumAutomatedApp AsSeleniumApp(IAutomatedApp app)
	{
		if (app is not SeleniumAutomatedApp seleniumApp)
			throw new ArgumentException($"Expected {nameof(SeleniumAutomatedApp)} but got {app.GetType().Name}", nameof(app));
		return seleniumApp;
	}
}
