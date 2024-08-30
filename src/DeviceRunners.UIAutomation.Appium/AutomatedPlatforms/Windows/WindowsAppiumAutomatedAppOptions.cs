using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public WindowsAppiumAutomatedAppOptions(string key, AppiumOptions appiumOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, new WindowsAppiumDriverFactory(), new WindowsAppiumByFactory(), appiumOptions, commands)
	{
	}
}
