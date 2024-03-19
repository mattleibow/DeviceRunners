using OpenQA.Selenium.Appium;

namespace DeviceRunners.Appium;

public class AppiumDriverManagerOptions
{
	public AppiumOptions Options { get; set; } = new AppiumOptions();

	public IAppiumDriverFactory? DriverFactory { get; set; }
}
