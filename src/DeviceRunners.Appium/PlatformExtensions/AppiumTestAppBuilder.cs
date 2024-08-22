using OpenQA.Selenium.Appium;

namespace DeviceRunners.Appium;

public abstract class AppiumTestAppBuilder
{
	public AppiumOptions AppiumOptions { get; } = new();
}
