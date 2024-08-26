using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public AndroidAppiumAutomatedAppOptions(string key, IAppiumDriverFactory driverFactory, AppiumOptions appiumOptions)
		: base(key, driverFactory, appiumOptions)
	{
	}
}
