using OpenQA.Selenium.Appium;

namespace DeviceRunners.Appium;

public interface IAppiumDriverFactory
{
	AppiumDriver CreateDriver(AppiumDriverManagerOptions options, AppiumServiceManager appium);
}
