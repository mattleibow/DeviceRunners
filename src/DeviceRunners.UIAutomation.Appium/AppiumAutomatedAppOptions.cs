using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public abstract class AppiumAutomatedAppOptions : IAutomatedAppOptions
{
	public AppiumAutomatedAppOptions(string key, AppiumOptions driverOptions, IAppiumDriverFactory driverFactory, IAppiumByFactory byFactory, IReadOnlyList<IAutomatedAppCommand> commands)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentNullException.ThrowIfNull(driverFactory, nameof(driverFactory));
		ArgumentNullException.ThrowIfNull(byFactory, nameof(byFactory));
		ArgumentNullException.ThrowIfNull(driverOptions, nameof(driverOptions));

		Key = key;
		DriverOptions = driverOptions;
		DriverFactory = driverFactory;
		ByFactory = byFactory;
		Commands = commands;
	}

	public string Key { get; }

	public AppiumOptions DriverOptions { get; }

	public IAppiumDriverFactory DriverFactory { get; }

	public IAppiumByFactory ByFactory { get; }

	public IReadOnlyList<IAutomatedAppCommand> Commands { get; }
}
