using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace DeviceRunners.Appium;

public static class AppiumDriverExtensions
{
	public static AppState GetAppState(this AppiumDriver driver)
	{
		var automationName = driver.GetAutomationName()?.ToLowerInvariant();

		return automationName switch
		{
			"windows" => AppState.NotInstalled,
			"uiautomator2" => driver.GetAppState(driver.Capabilities.GetCapability(AndroidMobileCapabilityType.AppPackage)?.ToString()),
			_ => throw new ArgumentException($"Unknown automation name: '{automationName}'."),
		};
	}

	public static string? GetAutomationName(this AppiumDriver driver) =>
		driver.Capabilities.GetCapability("automationName")?.ToString();
}
