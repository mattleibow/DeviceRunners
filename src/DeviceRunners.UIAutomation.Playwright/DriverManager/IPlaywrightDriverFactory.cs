using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public interface IPlaywrightDriverFactory
{
	IBrowser CreateDriver(PlaywrightServiceManager playwright, PlaywrightAutomatedAppOptions options);
}
