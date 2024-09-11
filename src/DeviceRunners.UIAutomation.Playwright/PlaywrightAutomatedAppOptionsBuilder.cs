namespace DeviceRunners.UIAutomation.Playwright;

public abstract class PlaywrightAutomatedAppOptionsBuilder : IAutomatedAppOptionsBuilder
{
	private readonly List<IAutomatedAppCommand> _commands = [];

	public PlaywrightAutomatedAppOptionsBuilder(string key, string browserType)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentException.ThrowIfNullOrWhiteSpace(browserType, nameof(browserType));

		Key = key;
		LaunchOptions.SetBrowserType(browserType);
	}

	public string Key { get; }

	public PlaywrightBrowserLaunchOptions LaunchOptions { get; } = new();

	public IReadOnlyList<IAutomatedAppCommand> Commands => _commands;

	void IAutomatedAppOptionsBuilder.AddCommand(IAutomatedAppCommand command) =>
		_commands.Add(command);

	public abstract PlaywrightAutomatedAppOptions Build();

	IAutomatedAppOptions IAutomatedAppOptionsBuilder.Build() => Build();
}
