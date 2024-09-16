using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

/// <summary>
/// This type represents an automated app that is driven by Selenium.
/// </summary>
public class SeleniumAutomatedApp : IAutomatedApp
{
	private readonly SeleniumAutomatedAppOptions _options;
	private readonly IDiagnosticLogger? _logger;

	public SeleniumAutomatedApp(SeleniumAutomationFramework selenium, SeleniumAutomatedAppOptions options, IDiagnosticLogger? logger = null)
	{
		Framework = selenium;
		_options = options;
		_logger = logger;
		DriverManager = new SeleniumDriverManager(_options);
		Commands = new AutomatedAppCommandManager(this, options.Commands);
	}

	public SeleniumAutomationFramework Framework { get; }

	public SeleniumDriverManager DriverManager { get; }

	public WebDriver Driver => DriverManager.Driver;

	public IAutomatedAppCommandManager Commands { get; }

	public SeleniumAutomatedAppElement FindElement(Action<IBy> by)
	{
		ArgumentNullException.ThrowIfNull(by);

		var SeleniumBy = _options.ByFactory.Create(this);
		by(SeleniumBy);

		var element = Driver.FindElement(SeleniumBy.ToBy());

		return new SeleniumAutomatedAppElement(this, (WebElement)element);
	}

	IAutomatedAppElement IContainsElements.FindElement(Action<IBy> by) =>
		FindElement(by);

	public IReadOnlyList<SeleniumAutomatedAppElement> FindElements(Action<IBy> by)
	{
		ArgumentNullException.ThrowIfNull(by);

		var SeleniumBy = _options.ByFactory.Create(this);
		by(SeleniumBy);

		var elements = Driver.FindElements(SeleniumBy.ToBy());

		return elements.Select(e => new SeleniumAutomatedAppElement(this, (WebElement)e)).ToList();
	}

	IReadOnlyList<IAutomatedAppElement> IContainsElements.FindElements(Action<IBy> by) =>
		FindElements(by);
}
