using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class MacAppiumDriverFactory : IAppiumDriverFactory
{
	public AppiumDriver CreateDriver(AppiumServiceManager appium, AppiumAutomatedAppOptions options) =>
		new MacDriver(appium.Service.ServiceUrl, options.DriverOptions);
}
