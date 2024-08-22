using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace DeviceRunners.Appium;

public class AndroidAppiumTestAppBuilder : AppiumTestAppBuilder
{
	public AndroidAppiumTestAppBuilder()
	{
		AppiumOptions.AutomationName = "UIAutomator2";
		AppiumOptions.PlatformName = "Android";
	}

	public AndroidAppiumTestAppBuilder UseAppPackageFilePath(string filePath)
	{
		AppiumOptions.App = filePath;
		return this;
	}

	public AndroidAppiumTestAppBuilder UseAppPackageName(string packageName, string activityName)
	{
		AppiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, packageName);
		AppiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, activityName);
		return this;
	}
}
