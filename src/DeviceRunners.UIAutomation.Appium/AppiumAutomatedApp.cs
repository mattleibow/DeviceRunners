using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

/// <summary>
/// This type represents an automated app that is driven by Appium.
/// </summary>
public class AppiumAutomatedApp : IAutomatedApp
{
	private readonly AppiumAutomatedAppOptions _options;
	private readonly IDiagnosticLogger? _logger;

	public AppiumAutomatedApp(AppiumAutomationFramework appium, AppiumAutomatedAppOptions options, IDiagnosticLogger? logger = null)
	{
		Framework = appium;
		_options = options;
		_logger = logger;
		DriverManager = new AppiumDriverManager(Framework.ServiceManager, _options);
		Commands = new AutomatedAppCommandManager(this, options.Commands);
	}

	public AppiumAutomationFramework Framework { get; }

	public AppiumServiceManager ServiceManager => Framework.ServiceManager;

	public AppiumDriverManager DriverManager { get; }

	public AppiumDriver Driver => DriverManager.Driver;

	public IAutomatedAppCommandManager Commands { get; }

	public AppiumAutomatedAppElement FindElement(Action<IBy> by)
	{
		ArgumentNullException.ThrowIfNull(by);

		var appiumBy = _options.ByFactory.Create(this);
		by(appiumBy);

		var element = Driver.FindElement(appiumBy.ToBy());

		return new AppiumAutomatedAppElement(this, element);
	}

	IAutomatedAppElement IContainsElements.FindElement(Action<IBy> by) =>
		FindElement(by);

	public IReadOnlyList<AppiumAutomatedAppElement> FindElements(Action<IBy> by)
	{
		ArgumentNullException.ThrowIfNull(by);

		var appiumBy = _options.ByFactory.Create(this);
		by(appiumBy);

		var elements = Driver.FindElements(appiumBy.ToBy());

		return elements.Select(e => new AppiumAutomatedAppElement(this, e)).ToList();
	}

	IReadOnlyList<IAutomatedAppElement> IContainsElements.FindElements(Action<IBy> by) =>
		FindElements(by);
}
