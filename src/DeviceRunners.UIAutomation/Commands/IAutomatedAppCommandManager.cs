namespace DeviceRunners.UIAutomation;

public interface IAutomatedAppCommandManager
{
	bool ContainsCommand(string commandName);

	object? Execute(string commandName, IReadOnlyDictionary<string, object>? parameters = null);
}
