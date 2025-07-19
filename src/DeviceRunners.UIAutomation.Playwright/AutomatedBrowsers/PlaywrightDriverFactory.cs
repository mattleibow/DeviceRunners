using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightDriverFactory : IPlaywrightDriverFactory
{
	public PlaywrightDriverFactory(string browserType)
	{
		BrowserType = browserType;
	}

	public string BrowserType { get; }

	public IBrowser CreateDriver(PlaywrightServiceManager playwright, PlaywrightAutomatedAppOptions options)
	{
		var type = playwright.Service[BrowserType];
		var browserTask = type.LaunchAsync(options.LaunchOptions.GetBrowserTypeLaunchOptions());
		var browser = browserTask.GetAwaiter().GetResult();
		return browser;
	}
}
