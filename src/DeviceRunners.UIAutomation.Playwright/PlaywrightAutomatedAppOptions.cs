namespace DeviceRunners.UIAutomation.Playwright;

public abstract class PlaywrightAutomatedAppOptions : IAutomatedAppOptions
{
	public PlaywrightAutomatedAppOptions(string key, IPlaywrightBrowserLaunchOptions launchOptions, IPlaywrightDriverFactory driverFactory, IPlaywrightByFactory byFactory, IReadOnlyList<IAutomatedAppCommand> commands)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentNullException.ThrowIfNull(launchOptions, nameof(launchOptions));
		ArgumentNullException.ThrowIfNull(driverFactory, nameof(driverFactory));
		ArgumentNullException.ThrowIfNull(byFactory, nameof(byFactory));

		Key = key;
		LaunchOptions = launchOptions;
		DriverFactory = driverFactory;
		ByFactory = byFactory;
		Commands = commands;
	}

	public string Key { get; }

	public IPlaywrightBrowserLaunchOptions LaunchOptions { get; }

	public IPlaywrightDriverFactory DriverFactory { get; }

	public IPlaywrightByFactory ByFactory { get; }

	public IReadOnlyList<IAutomatedAppCommand> Commands { get; }
}
