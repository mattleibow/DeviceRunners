using OpenQA.Selenium.Appium;

namespace DeviceRunners.Appium;

public class WindowsAppiumTestAppBuilder : AppiumTestAppBuilder
{
	public WindowsAppiumTestAppBuilder()
	{
		AppiumOptions.AutomationName = "windows";
		AppiumOptions.PlatformName = "Windows";
		AppiumOptions.DeviceName = "WindowsPC";
	}

	public WindowsAppiumTestAppBuilder UseApp(string executablePathOrAppId)
	{
		AppiumOptions.App = executablePathOrAppId;
		return this;
	}
}
