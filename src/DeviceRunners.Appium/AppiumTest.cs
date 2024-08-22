using System.Collections.Concurrent;

namespace DeviceRunners.Appium;

public class AppiumTest : IDisposable
{
	private readonly IReadOnlyDictionary<string, AppiumDriverManagerOptions> _driverOptions;
	private readonly IAppiumDiagnosticLogger _logger;
	private readonly AppiumServiceManager _serviceManager;

	private readonly ConcurrentDictionary<string, AppiumTestApp> _apps = new();

	bool _disposed;

	public AppiumTest(AppiumServiceManagerOptions serviceOptions, IReadOnlyDictionary<string, AppiumDriverManagerOptions> driverOptions, IAppiumDiagnosticLogger logger)
	{
		_driverOptions = driverOptions;
		_logger = logger;
		_serviceManager = new AppiumServiceManager(serviceOptions, logger);
	}

	public AppiumServiceManager ServiceManager
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, typeof(AppiumTest));

			return _serviceManager;
		}
	}

	public AppiumTestApp GetApp(string appKey, bool restartDriver = true)
	{
		ObjectDisposedException.ThrowIf(_disposed, typeof(AppiumTest));

		return _apps.AddOrUpdate(
			appKey,
			_ => new AppiumTestApp(this, _driverOptions[appKey], _logger),
			(_, old) =>
			{
				if (restartDriver)
					old.DriverManager.Restart();
				return old;
			});
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		foreach (var app in _apps.Values)
		{
			app.DriverManager.Dispose();
		}
		_apps.Clear();

		_serviceManager.Dispose();
	}
}
