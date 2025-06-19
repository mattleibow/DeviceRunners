using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public abstract class SeleniumAutomatedAppOptionsBuilder : IAutomatedAppOptionsBuilder
{
	private readonly List<IAutomatedAppCommand> _commands = [];

	public SeleniumAutomatedAppOptionsBuilder(string key, DriverOptions driverOptions)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentNullException.ThrowIfNull(driverOptions, nameof(driverOptions));

		Key = key;
		DriverOptions = driverOptions;
	}

	public string Key { get; }

	public DriverOptions DriverOptions { get; }

	public IReadOnlyList<IAutomatedAppCommand> Commands => _commands;

	void IAutomatedAppOptionsBuilder.AddCommand(IAutomatedAppCommand command) =>
		_commands.Add(command);

	public abstract SeleniumAutomatedAppOptions Build();

	IAutomatedAppOptions IAutomatedAppOptionsBuilder.Build() => Build();
}
