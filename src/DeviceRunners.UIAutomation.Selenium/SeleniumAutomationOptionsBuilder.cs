namespace DeviceRunners.UIAutomation.Selenium;

/// <summary>
/// This type is responsible for building the options for the Selenium automation framework.
/// </summary>
public class SeleniumAutomationOptionsBuilder
{
	private readonly List<IDiagnosticLogger> _loggers = [];
	private readonly Dictionary<string, SeleniumAutomatedAppOptions> _apps = [];

	public SeleniumAutomationOptionsBuilder AddLogger(IDiagnosticLogger logger)
	{
		_loggers.Add(logger);

		return this;
	}

	public SeleniumAutomationOptionsBuilder AddApp(string key, SeleniumAutomatedAppOptions options)
	{
		if (_apps.TryGetValue(key, out var existing))
			throw new InvalidOperationException($"App with key '{key}' was already added: {existing}");

		_apps[key] = options;

		return this;
	}

	internal IReadOnlyCollection<SeleniumAutomatedAppOptions> Apps => _apps.Values;

	internal IReadOnlyCollection<IDiagnosticLogger> Loggers => _loggers;
}
