using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public abstract class AppiumAutomatedAppOptions : IAutomatedAppOptions
{
	public AppiumAutomatedAppOptions(string key, IAppiumDriverFactory driverFactory, IAppiumByFactory byFactory, AppiumOptions appiumOptions, IReadOnlyList<IAutomatedAppCommand> commands)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentNullException.ThrowIfNull(driverFactory, nameof(driverFactory));
		ArgumentNullException.ThrowIfNull(byFactory, nameof(byFactory));
		ArgumentNullException.ThrowIfNull(appiumOptions, nameof(appiumOptions));

		Key = key;
		DriverFactory = driverFactory;
		ByFactory = byFactory;
		AppiumOptions = appiumOptions;
		Commands = commands;
	}

	public string Key { get; }

	public IAppiumDriverFactory DriverFactory { get; }

	public IAppiumByFactory ByFactory { get; }
	
	public AppiumOptions AppiumOptions { get; }

	public IReadOnlyList<IAutomatedAppCommand> Commands { get; }
}
