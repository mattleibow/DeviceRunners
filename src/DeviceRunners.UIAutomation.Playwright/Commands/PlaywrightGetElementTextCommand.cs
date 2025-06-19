using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightGetElementTextCommand : PlaywrightElementCommand
{
	public PlaywrightGetElementTextCommand()
		: base(CommonCommandNames.GetElementText)
	{
	}

	public override object? Execute(PlaywrightAutomatedApp app, ILocator playwrightElement, IReadOnlyDictionary<string, object> parameters) =>
		playwrightElement.TextContentAsync().GetAwaiter().GetResult();
}
