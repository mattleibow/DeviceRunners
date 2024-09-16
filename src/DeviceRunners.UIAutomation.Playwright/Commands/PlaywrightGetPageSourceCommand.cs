namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightGetPageSourceCommand : AutomatedAppCommand<PlaywrightAutomatedApp>
{
	public PlaywrightGetPageSourceCommand()
		: base(CommonCommandNames.GetPageSource)
	{
	}

	public override object? Execute(PlaywrightAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null) =>
		app.Page.ContentAsync().GetAwaiter().GetResult();
}
