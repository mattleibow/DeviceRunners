using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumAutomatedAppOptionsBuilder : AppiumAutomatedAppOptionsBuilder
{
	public AndroidAppiumAutomatedAppOptionsBuilder(string key)
		: base(key)
	{
		AppiumOptions.AutomationName = "UIAutomator2";
		AppiumOptions.PlatformName = "Android";
	}

	public AndroidAppiumAutomatedAppOptionsBuilder UsePackageName(string packageName)
	{
		AppiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, packageName);
		return this;
	}

	public AndroidAppiumAutomatedAppOptionsBuilder UseActivityName(string activityName)
	{
		AppiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, activityName);
		return this;
	}

	public AndroidAppiumAutomatedAppOptionsBuilder UseAppPackagePath(string path)
	{
		AppiumOptions.App = path;
		return this;
	}

	public override AppiumAutomatedAppOptions Build() =>
		new AndroidAppiumAutomatedAppOptions(Key, new AndroidAppiumDriverFactory(), AppiumOptions);
}
