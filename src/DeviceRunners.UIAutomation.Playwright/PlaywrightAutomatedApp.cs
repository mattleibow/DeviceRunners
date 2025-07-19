using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

/// <summary>
/// This type represents an automated app that is driven by Playwright.
/// </summary>
public class PlaywrightAutomatedApp : IAutomatedApp
{
	private readonly PlaywrightAutomatedAppOptions _options;
	private readonly IDiagnosticLogger? _logger;

	public PlaywrightAutomatedApp(PlaywrightAutomationFramework playwright, PlaywrightAutomatedAppOptions options, IDiagnosticLogger? logger = null)
	{
		Framework = playwright;
		_options = options;
		_logger = logger;
		DriverManager = new PlaywrightDriverManager(Framework.ServiceManager, _options);
		Commands = new AutomatedAppCommandManager(this, options.Commands);
	}

	public PlaywrightAutomationFramework Framework { get; }

	public PlaywrightServiceManager ServiceManager => Framework.ServiceManager;

	public PlaywrightDriverManager DriverManager { get; }

	public IPage Page => DriverManager.Page;

	public IAutomatedAppCommandManager Commands { get; }

	public PlaywrightAutomatedAppElement FindElement(Action<IBy> by)
	{
		ArgumentNullException.ThrowIfNull(by);

		var playwrightBy = _options.ByFactory.Create(this);
		by(playwrightBy);

		var elements = playwrightBy.Locate(Page);

		return new PlaywrightAutomatedAppElement(this, elements.First);
	}

	IAutomatedAppElement IContainsElements.FindElement(Action<IBy> by) =>
		FindElement(by);

	public IReadOnlyList<PlaywrightAutomatedAppElement> FindElements(Action<IBy> by)
	{
		ArgumentNullException.ThrowIfNull(by);

		var playwrightBy = _options.ByFactory.Create(this);
		by(playwrightBy);

		var elements = playwrightBy.Locate(Page);
		var all = elements.AllAsync().GetAwaiter().GetResult();

		return all.Select(e => new PlaywrightAutomatedAppElement(this, e)).ToList();
	}

	IReadOnlyList<IAutomatedAppElement> IContainsElements.FindElements(Action<IBy> by) =>
		FindElements(by);
}
