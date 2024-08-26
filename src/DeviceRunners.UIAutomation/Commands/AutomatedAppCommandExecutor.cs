﻿namespace DeviceRunners.UIAutomation;

public class AutomatedAppCommandExecutor : IAutomatedAppCommandManager
{
	private readonly Stack<IAutomatedAppCommand> _commands = new();

	public AutomatedAppCommandExecutor(IAutomatedApp app)
	{
		App = app;
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
