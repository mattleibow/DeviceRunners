using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public AndroidAppiumAutomatedAppOptions(string key, AppiumOptions driverOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, driverOptions, new AndroidAppiumDriverFactory(), new AppiumByFactory(), commands)
	{
	}
}
