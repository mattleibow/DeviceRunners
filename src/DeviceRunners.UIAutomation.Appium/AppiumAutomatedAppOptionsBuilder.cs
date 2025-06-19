using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public abstract class AppiumAutomatedAppOptionsBuilder : IAutomatedAppOptionsBuilder
{
	private readonly List<IAutomatedAppCommand> _commands = [];

	public AppiumAutomatedAppOptionsBuilder(string key)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		Key = key;
	}

	public string Key { get; }

	public AppiumOptions DriverOptions { get; } = new();

	public IReadOnlyList<IAutomatedAppCommand> Commands => _commands;

	void IAutomatedAppOptionsBuilder.AddCommand(IAutomatedAppCommand command) =>
		_commands.Add(command);

	public abstract AppiumAutomatedAppOptions Build();

	IAutomatedAppOptions IAutomatedAppOptionsBuilder.Build() => Build();
}
