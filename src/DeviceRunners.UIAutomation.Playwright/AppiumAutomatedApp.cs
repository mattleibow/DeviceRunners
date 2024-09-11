namespace DeviceRunners.UIAutomation.Playwright;

/// <summary>
/// This type represents an automated app that is driven by Playwright.
/// </summary>
public class PlaywrightAutomatedApp : IAutomatedApp
{
	//private readonly PlaywrightAutomatedAppOptions _options;
	//private readonly IPlaywrightDiagnosticLogger? _logger;

	//public PlaywrightAutomatedApp(PlaywrightAutomationFramework playwright, PlaywrightAutomatedAppOptions options, IPlaywrightDiagnosticLogger? logger = null)
	//{
	//	Framework = playwright;
	//	_options = options;
	//	_logger = logger;
	//	DriverManager = new PlaywrightDriverManager(Framework.ServiceManager, _options);
	//	Commands = new AutomatedAppCommandManager(this, options.Commands);
	//}

	//public PlaywrightAutomationFramework Framework { get; }

	//public PlaywrightServiceManager ServiceManager => Framework.ServiceManager;

	//public PlaywrightDriverManager DriverManager { get; }

	//public PlaywrightDriver Driver => DriverManager.Driver;

	public IAutomatedAppCommandManager Commands { get; }

	IReadOnlyList<IAutomatedAppElement> IContainsElements.FindElements(Action<IBy> by) => null;
	IAutomatedAppElement IContainsElements.FindElement(Action<IBy> by) => null;

	//public PlaywrightAutomatedAppElement FindElement(Action<IBy> by)
	//{
	//	ArgumentNullException.ThrowIfNull(by);

	//	var playwrightBy = _options.ByFactory.Create(this);
	//	by(playwrightBy);

	//	var element = Driver.FindElement(playwrightBy.ToBy());

	//	return new PlaywrightAutomatedAppElement(this, element);
	//}

	//IAutomatedAppElement IContainsElements.FindElement(Action<IBy> by) =>
	//	FindElement(by);

	//public IReadOnlyList<PlaywrightAutomatedAppElement> FindElements(Action<IBy> by)
	//{
	//	ArgumentNullException.ThrowIfNull(by);

	//	var playwrightBy = _options.ByFactory.Create(this);
	//	by(playwrightBy);

	//	var elements = Driver.FindElements(playwrightBy.ToBy());

	//	return elements.Select(e => new PlaywrightAutomatedAppElement(this, e)).ToList();
	//}

	//IReadOnlyList<IAutomatedAppElement> IContainsElements.FindElements(Action<IBy> by) =>
	//	FindElements(by);
}
