using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumDriverFactory : IAppiumDriverFactory
{
	public AppiumDriver CreateDriver(AppiumServiceManager appium, AppiumAutomatedAppOptions options) =>
		new AndroidDriver(appium.Service.ServiceUrl, options.DriverOptions);
}
