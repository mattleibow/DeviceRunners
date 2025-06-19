using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public abstract class SeleniumAutomatedAppOptions : IAutomatedAppOptions
{
	public SeleniumAutomatedAppOptions(string key, DriverOptions driverOptions, ISeleniumDriverFactory driverFactory, ISeleniumByFactory byFactory, IReadOnlyList<IAutomatedAppCommand> commands)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentNullException.ThrowIfNull(driverOptions, nameof(driverOptions));
		ArgumentNullException.ThrowIfNull(driverFactory, nameof(driverFactory));
		ArgumentNullException.ThrowIfNull(byFactory, nameof(byFactory));

		Key = key;
		DriverOptions = driverOptions;
		DriverFactory = driverFactory;
		ByFactory = byFactory;
		Commands = commands;
	}

	public string Key { get; }

	public DriverOptions DriverOptions { get; }

	public ISeleniumDriverFactory DriverFactory { get; }

	public ISeleniumByFactory ByFactory { get; }

	public IReadOnlyList<IAutomatedAppCommand> Commands { get; }
}
