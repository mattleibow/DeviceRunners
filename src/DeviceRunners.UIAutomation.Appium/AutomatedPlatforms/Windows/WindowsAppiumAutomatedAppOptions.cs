using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public WindowsAppiumAutomatedAppOptions(string key, AppiumOptions driverOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, driverOptions, new WindowsAppiumDriverFactory(), new WindowsAppiumByFactory(), commands)
	{
	}
}
