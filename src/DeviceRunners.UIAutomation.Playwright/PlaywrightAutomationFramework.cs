namespace DeviceRunners.UIAutomation.Playwright;

/// <summary>
/// This type is responsible for creating and managing Playwright automated app instances.
/// </summary>
public class PlaywrightAutomationFramework : IAutomationFramework
{
	private readonly PlaywrightServiceManagerOptions _options;
	private readonly IReadOnlyList<PlaywrightAutomatedAppOptions> _apps;
	private readonly IDiagnosticLogger? _logger;
	private PlaywrightServiceManager? _serviceManager;
	private bool _disposed;

	public PlaywrightAutomationFramework(PlaywrightServiceManagerOptions options, IEnumerable<PlaywrightAutomatedAppOptions> apps, IDiagnosticLogger? logger = null)
	{
		_options = options;
		_apps = apps.ToList();
		_logger = logger;
	}

	public PlaywrightServiceManager ServiceManager
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, typeof(PlaywrightAutomationFramework));
			return _serviceManager ??= new PlaywrightServiceManager(_options, _logger);
		}
	}

	public IReadOnlyList<PlaywrightAutomatedAppOptions> AvailableApps => _apps;

	public IAutomatedApp CreateApp(PlaywrightAutomatedAppOptions options)
	{
		return new PlaywrightAutomatedApp(this, options, _logger);
	}

	public void StartApp(PlaywrightAutomatedApp app)
	{
		app.DriverManager.StartDriver();
	}

	public void StopApp(PlaywrightAutomatedApp app)
	{
		app.DriverManager.ShutdownDriver();
	}

	public void RestartApp(PlaywrightAutomatedApp app)
	{
		app.DriverManager.RestartDriver();
	}

	IReadOnlyList<IAutomatedAppOptions> IAutomationFramework.AvailableApps => _apps;

	IAutomatedApp IAutomationFramework.CreateApp(IAutomatedAppOptions options)
	{
		if (options is not PlaywrightAutomatedAppOptions playwrightOptions)
			throw new ArgumentException($"Expected {nameof(PlaywrightAutomatedAppOptions)} but got {options.GetType().Name}", nameof(options));

		return CreateApp(playwrightOptions);
	}

	void IAutomationFramework.StartApp(IAutomatedApp app) => StartApp(AsPlaywrightApp(app));

	void IAutomationFramework.StopApp(IAutomatedApp app) => StopApp(AsPlaywrightApp(app));

	void IAutomationFramework.RestartApp(IAutomatedApp app) => RestartApp(AsPlaywrightApp(app));

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		_serviceManager?.Dispose();
	}

	private static PlaywrightAutomatedApp AsPlaywrightApp(IAutomatedApp app)
	{
		if (app is not PlaywrightAutomatedApp playwrightApp)
			throw new ArgumentException($"Expected {nameof(PlaywrightAutomatedApp)} but got {app.GetType().Name}", nameof(app));
		return playwrightApp;
	}
}
