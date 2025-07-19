using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace DeviceRunners.UIAutomation.Appium;

public class MacAppiumAutomatedAppOptionsBuilder : AppiumAutomatedAppOptionsBuilder
{
	public MacAppiumAutomatedAppOptionsBuilder(string key)
		: base(key)
	{
		DriverOptions.AutomationName = "mac2";
		DriverOptions.PlatformName = "mac";
	}

	public MacAppiumAutomatedAppOptionsBuilder UseBundleId(string bundleId)
	{
		DriverOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, bundleId);
		return this;
	}

	public MacAppiumAutomatedAppOptionsBuilder UseAppExecutablePath(string executablePath)
	{
		DriverOptions.App = executablePath;
		return this;
	}

	public override AppiumAutomatedAppOptions Build() =>
		new MacAppiumAutomatedAppOptions(Key, DriverOptions, Commands);
}
