using DeviceRunners.UIAutomation.Appium;
using DeviceRunners.UIAutomation.Playwright;

using NUnit.Framework.Internal;
using Microsoft.Playwright;

namespace DeviceTestingKitApp.UITests.NUnitTests;

public class PlaywrightServerTests
{
	public PlaywrightServerTests()
	{
	}

	[Test]
	public async Task IsReady()
	{
		using var playwright = await Playwright.CreateAsync();

		await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge" });

		var page = await browser.NewPageAsync();
		
		await page.GotoAsync("https://playwright.dev/dotnet");
		
		await page.ScreenshotAsync(new()
		{
			Path = "screenshot.png"
		});
	}
}
