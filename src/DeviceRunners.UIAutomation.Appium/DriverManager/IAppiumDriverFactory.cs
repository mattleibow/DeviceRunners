using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public interface IAppiumDriverFactory
{
	AppiumDriver CreateDriver(AppiumServiceManager appium, AppiumAutomatedAppOptions options);
}
