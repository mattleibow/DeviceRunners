namespace DeviceRunners.UIAutomation;

public class AutomatedAppCommandManager : IAutomatedAppCommandManager
{
	private readonly IReadOnlyList<IAutomatedAppCommand> _commands;

	public AutomatedAppCommandManager(IAutomatedApp app, IReadOnlyList<IAutomatedAppCommand> commands)
	{
		App = app;
		_commands = commands;
	}

	public IAutomatedApp App { get; }

	public bool ContainsCommand(string commandName) =>
		GetCommand(commandName) is not null;

	public object? Execute(string commandName, IReadOnlyDictionary<string, object>? parameters = null)
	{
		var command = GetCommand(commandName) ?? throw new KeyNotFoundException($"Command '{commandName}' was not found.");
		return command.Execute(App, parameters);
	}

	private IAutomatedAppCommand? GetCommand(string commandName) =>
		_commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
}
