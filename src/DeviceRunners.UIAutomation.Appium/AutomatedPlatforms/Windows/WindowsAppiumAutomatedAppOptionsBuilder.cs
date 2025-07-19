using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumAutomatedAppOptionsBuilder : AppiumAutomatedAppOptionsBuilder
{
	public WindowsAppiumAutomatedAppOptionsBuilder(string key)
		: base(key)
	{
		DriverOptions.AutomationName = "windows";
		DriverOptions.PlatformName = "Windows";
		DriverOptions.DeviceName = "WindowsPC";
	}

	public WindowsAppiumAutomatedAppOptionsBuilder UseAppId(string appId)
	{
		DriverOptions.App = appId;
		return this;
	}

	public WindowsAppiumAutomatedAppOptionsBuilder UseAppExecutablePath(string executablePath)
	{
		DriverOptions.App = executablePath;
		return this;
	}

	public override AppiumAutomatedAppOptions Build() =>
		new WindowsAppiumAutomatedAppOptions(Key, DriverOptions, Commands);
}
