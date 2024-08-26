using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public WindowsAppiumAutomatedAppOptions(string key, IAppiumDriverFactory driverFactory, AppiumOptions appiumOptions)
		: base(key, driverFactory, appiumOptions)
	{
	}
}
