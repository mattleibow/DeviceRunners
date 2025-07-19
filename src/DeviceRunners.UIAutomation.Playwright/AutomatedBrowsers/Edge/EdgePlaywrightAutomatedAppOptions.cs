using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class EdgePlaywrightAutomatedAppOptions : PlaywrightAutomatedAppOptions
{
	public EdgePlaywrightAutomatedAppOptions(string key, IPlaywrightBrowserLaunchOptions launchOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, launchOptions, new PlaywrightDriverFactory(BrowserType.Chromium), new PlaywrightByFactory(), commands)
	{
	}
}
