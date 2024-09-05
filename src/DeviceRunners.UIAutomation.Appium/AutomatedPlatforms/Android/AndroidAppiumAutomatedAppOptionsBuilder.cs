using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumAutomatedAppOptionsBuilder : AppiumAutomatedAppOptionsBuilder
{
	public AndroidAppiumAutomatedAppOptionsBuilder(string key)
		: base(key)
	{
		DriverOptions.AutomationName = "UIAutomator2";
		DriverOptions.PlatformName = "Android";
	}

	public AndroidAppiumAutomatedAppOptionsBuilder UsePackageName(string packageName)
	{
		DriverOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, packageName);
		return this;
	}

	public AndroidAppiumAutomatedAppOptionsBuilder UseActivityName(string activityName)
	{
		DriverOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, activityName);
		return this;
	}

	public AndroidAppiumAutomatedAppOptionsBuilder UseAppPackagePath(string path)
	{
		DriverOptions.App = path;
		return this;
	}

	public override AppiumAutomatedAppOptions Build() =>
		new AndroidAppiumAutomatedAppOptions(Key, DriverOptions, Commands);
}
