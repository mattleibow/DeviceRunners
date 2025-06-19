using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class EdgePlaywrightAutomatedAppOptionsBuilder : PlaywrightAutomatedAppOptionsBuilder
{
	public EdgePlaywrightAutomatedAppOptionsBuilder(string key)
		: base(key, BrowserType.Chromium)
	{
		LaunchOptions.GetOrAddBrowserTypeLaunchOptions().Channel = "msedge";
	}

	public override PlaywrightAutomatedAppOptions Build() =>
		new EdgePlaywrightAutomatedAppOptions(Key, LaunchOptions, Commands);
}
