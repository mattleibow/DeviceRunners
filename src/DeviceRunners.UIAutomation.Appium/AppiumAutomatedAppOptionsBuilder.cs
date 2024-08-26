using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public abstract class AppiumAutomatedAppOptionsBuilder : IAutomatedAppOptionsBuilder
{
	private readonly Stack<IAutomatedAppCommand> _commands = new();

	public AppiumAutomatedAppOptionsBuilder(string key)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		Key = key;
	}

	public string Key { get; }

	public AppiumOptions AppiumOptions { get; } = new();

	void IAutomatedAppOptionsBuilder.AddCommand(IAutomatedAppCommand command) =>
		_commands.Push(command);

	public abstract AppiumAutomatedAppOptions Build();

	IAutomatedAppOptions IAutomatedAppOptionsBuilder.Build() => Build();
}
