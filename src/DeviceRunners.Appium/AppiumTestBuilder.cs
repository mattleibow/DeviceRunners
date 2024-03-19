namespace DeviceRunners.Appium;

public class AppiumTestBuilder
{
	private readonly AppiumServiceManagerOptions _serviceOptions = new();
	private readonly Dictionary<string, AppiumDriverManagerOptions> _platformOptions = [];
	private readonly List<IAppiumDiagnosticLogger> _loggers = [];

	public static AppiumTestBuilder Create() => new AppiumTestBuilder();

	public AppiumTestBuilder UseServiceAddress(string hostAddress = AppiumServiceManagerOptions.DefaultHostAddress, int port = AppiumServiceManagerOptions.DefaultHostPort)
	{
		_serviceOptions.HostAddress = hostAddress;
		_serviceOptions.HostPort = port;
		return this;
	}

	public AppiumTestBuilder AddApp(string key, AppiumDriverManagerOptions options)
	{
		if (_platformOptions.ContainsKey(key))
			throw new InvalidOperationException($"App with key '{key}' already added.");

		_platformOptions[key] = options;

		return this;
	}

	public AppiumTestBuilder AddLogger(IAppiumDiagnosticLogger logger)
	{
		_loggers.Add(logger);

		return this;
	}

	public AppiumTest Build() =>
		new AppiumTest(_serviceOptions, _platformOptions, new CompositeLogger(_loggers));

	class CompositeLogger : IAppiumDiagnosticLogger
	{
		private readonly IReadOnlyList<IAppiumDiagnosticLogger> _loggers;

		public CompositeLogger(IReadOnlyList<IAppiumDiagnosticLogger> loggers) =>
			_loggers = loggers;

		public void Log(string message)
		{
			foreach (var logger in _loggers)
				logger.Log(message);
		}
	}
}
