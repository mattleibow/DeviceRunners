using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class MacAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public MacAppiumAutomatedAppOptions(string key, AppiumOptions driverOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, driverOptions, new MacAppiumDriverFactory(), new AppiumByFactory(), commands)
	{
	}
}
