using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public abstract class AppiumAutomatedAppOptions : IAutomatedAppOptions
{
	public AppiumAutomatedAppOptions(string key, IAppiumDriverFactory driverFactory, AppiumOptions appiumOptions)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentNullException.ThrowIfNull(driverFactory, nameof(driverFactory));
		ArgumentNullException.ThrowIfNull(appiumOptions, nameof(appiumOptions));

		Key = key;
		DriverFactory = driverFactory;
		AppiumOptions = appiumOptions;
	}

	public string Key { get; }

	public IAppiumDriverFactory DriverFactory { get; }
	
	public AppiumOptions AppiumOptions { get; }
}
