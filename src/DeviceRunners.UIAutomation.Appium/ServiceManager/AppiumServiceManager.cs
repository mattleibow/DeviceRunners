using OpenQA.Selenium.Appium.Service;

namespace DeviceRunners.UIAutomation.Appium;

/// <summary>
/// This type is responsible for managing the Appium server lifecycle.
/// </summary>
public class AppiumServiceManager : IDisposable
{
	private readonly AppiumServiceManagerOptions _options;
	private readonly IDiagnosticLogger? _logger;
	private readonly AppiumLocalService _appiumService;

	public AppiumServiceManager(AppiumServiceManagerOptions options, IDiagnosticLogger? logger = null)
	{
		_options = options;
		_logger = logger;
		_appiumService = BuildAppiumServer();
		StartAppiumServer();
	}

	public AppiumLocalService Service => _appiumService;

	public bool IsRunning => Service.IsRunning;

	public Uri ServiceUrl => Service.ServiceUrl;

	private AppiumLocalService BuildAppiumServer()
	{
		_logger?.Log("Starting Appium server...");

		var args = new OpenQA.Selenium.Appium.Service.Options.OptionCollector()
			.AddArguments(new KeyValuePair<string, string>("--log-no-colors", null!));

		var builder = new AppiumServiceBuilder()
			.WithIPAddress(_options.HostAddress)
			.WithArguments(args);

		if (_options.HostPort <= 0)
			builder.UsingAnyFreePort();
		else
			builder.UsingPort(_options.HostPort);

		if (!string.IsNullOrEmpty(_options.LogFile))
			builder.WithLogFile(new FileInfo(_options.LogFile));

		var service = builder.Build();

		service.OutputDataReceived += (_, e) =>
		{
			_logger?.Log($"Appium data received: {e.Data}");
		};

		return service;
	}

	private void StartAppiumServer()
	{
		var startTicks = Environment.TickCount;

		_appiumService.Start();

		while (!_appiumService.IsRunning)
		{
			if (TimeSpan.FromMilliseconds(Environment.TickCount - startTicks) >= _options.ServerStartWaitDelay)
			{
				_logger?.Log($"Appium server did not start within the timeout period: {_options.ServerStartWaitDelay}");
				throw new TimeoutException($"Appium server did not start within the timeout period: {_options.ServerStartWaitDelay}");
			}

			Thread.Sleep(1000);
		}

		var delta = TimeSpan.FromMilliseconds(Environment.TickCount - startTicks).TotalSeconds;

		_logger?.Log($"Appium server started in {delta} seconds.");
	}

	public void Dispose()
	{
		_appiumService.Dispose();
	}
}
