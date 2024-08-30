using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumAutomatedAppOptions : AppiumAutomatedAppOptions
{
	public AndroidAppiumAutomatedAppOptions(string key, AppiumOptions appiumOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, new AndroidAppiumDriverFactory(), new AppiumByFactory(), appiumOptions, commands)
	{
	}
}
