using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumAutomatedAppOptionsBuilder : AppiumAutomatedAppOptionsBuilder
{
	public WindowsAppiumAutomatedAppOptionsBuilder(string key)
		: base(key)
	{
		AppiumOptions.AutomationName = "windows";
		AppiumOptions.PlatformName = "Windows";
		AppiumOptions.DeviceName = "WindowsPC";
	}

	public WindowsAppiumAutomatedAppOptionsBuilder UseAppId(string appId)
	{
		AppiumOptions.App = appId;
		return this;
	}

	public WindowsAppiumAutomatedAppOptionsBuilder UseExecutablePath(string executablePath)
	{
		AppiumOptions.App = executablePath;
		return this;
	}

	public override AppiumAutomatedAppOptions Build() =>
		new WindowsAppiumAutomatedAppOptions(Key, new WindowsAppiumDriverFactory(), AppiumOptions);
}
