using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumDriverFactory : IAppiumDriverFactory
{
	public AppiumDriver CreateDriver(AppiumServiceManager appium, AppiumAutomatedAppOptions options) =>
		new WindowsDriver(appium.Service.ServiceUrl, options.DriverOptions);
}
