//namespace DeviceRunners.UIAutomation.Playwright;

///// <summary>
///// This type is responsible for building the options for the Playwright automation framework.
///// </summary>
//public class PlaywrightAutomationOptionsBuilder
//{
//	private readonly List<IPlaywrightDiagnosticLogger> _loggers = [];
//	private readonly Dictionary<string, PlaywrightAutomatedAppOptions> _apps = [];

//	public PlaywrightAutomationOptionsBuilder UseServiceAddress(
//		string hostAddress = PlaywrightServiceManagerOptions.DefaultHostAddress,
//		int port = PlaywrightServiceManagerOptions.DefaultHostPort)
//	{
//		Options.HostAddress = hostAddress;
//		Options.HostPort = port;

//		return this;
//	}

//	public PlaywrightAutomationOptionsBuilder AddLogger(IPlaywrightDiagnosticLogger logger)
//	{
//		_loggers.Add(logger);
		
//		return this;
//	}

//	public PlaywrightAutomationOptionsBuilder AddApp(string key, PlaywrightAutomatedAppOptions options)
//	{
//		if (_apps.TryGetValue(key, out var existing))
//			throw new InvalidOperationException($"App with key '{key}' was already added: {existing}");

//		_apps[key] = options;

//		return this;
//	}

//	internal PlaywrightServiceManagerOptions Options { get; } = new();

//	internal IReadOnlyCollection<PlaywrightAutomatedAppOptions> Apps => _apps.Values;

//	internal IReadOnlyCollection<IPlaywrightDiagnosticLogger> Loggers => _loggers;
//}
