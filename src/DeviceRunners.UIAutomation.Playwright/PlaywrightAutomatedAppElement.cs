using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightAutomatedAppElement : IAutomatedAppElement
{
	public PlaywrightAutomatedAppElement(PlaywrightAutomatedApp app, ILocator element)
	{
		App = app;
		PlaywrightElement = element;
	}

	public PlaywrightAutomatedApp App { get; }

	public ILocator PlaywrightElement { get; }

	IAutomatedApp IAutomatedAppElement.App => App;
}
