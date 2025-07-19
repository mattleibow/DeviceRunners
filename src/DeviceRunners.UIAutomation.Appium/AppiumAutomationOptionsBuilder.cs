namespace DeviceRunners.UIAutomation.Appium;

/// <summary>
/// This type is responsible for building the options for the Appium automation framework.
/// </summary>
public class AppiumAutomationOptionsBuilder
{
	private readonly List<UIAutomation.IDiagnosticLogger> _loggers = [];
	private readonly Dictionary<string, AppiumAutomatedAppOptions> _apps = [];

	public AppiumAutomationOptionsBuilder UseServiceAddress(
		string hostAddress = AppiumServiceManagerOptions.DefaultHostAddress,
		int port = AppiumServiceManagerOptions.DefaultHostPort)
	{
		Options.HostAddress = hostAddress;
		Options.HostPort = port;

		return this;
	}

	public AppiumAutomationOptionsBuilder AddLogger(UIAutomation.IDiagnosticLogger logger)
	{
		_loggers.Add(logger);
		
		return this;
	}

	public AppiumAutomationOptionsBuilder AddApp(string key, AppiumAutomatedAppOptions options)
	{
		if (_apps.TryGetValue(key, out var existing))
			throw new InvalidOperationException($"App with key '{key}' was already added: {existing}");

		_apps[key] = options;

		return this;
	}

	internal AppiumServiceManagerOptions Options { get; } = new();

	internal IReadOnlyCollection<AppiumAutomatedAppOptions> Apps => _apps.Values;

	internal IReadOnlyCollection<UIAutomation.IDiagnosticLogger> Loggers => _loggers;
}
