namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumGetPageSourceCommand : AutomatedAppCommand<SeleniumAutomatedApp>
{
	public SeleniumGetPageSourceCommand()
		: base(CommonCommandNames.GetPageSource)
	{
	}

	public override object? Execute(SeleniumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		return app.Driver.PageSource;
	}
}
