namespace DeviceRunners.UIAutomation;

public static class AutomatedAppCommandManagerExtensions
{
	public static object? Execute(this IAutomatedAppCommandManager manager, string commandName, params (string Name, object Value)[] parameters) =>
		manager.Execute(commandName, parameters.ToDictionary());
}
