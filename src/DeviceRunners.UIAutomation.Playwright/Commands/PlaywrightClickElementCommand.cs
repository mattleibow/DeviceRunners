using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightClickElementCommand : PlaywrightElementCommand
{
	public PlaywrightClickElementCommand()
		: base(CommonCommandNames.ClickElement)
	{
	}

	public override object? Execute(PlaywrightAutomatedApp app, ILocator playwrightElement, IReadOnlyDictionary<string, object> parameters)
	{
		playwrightElement.ClickAsync().GetAwaiter().GetResult();
		return null;
	}
}
