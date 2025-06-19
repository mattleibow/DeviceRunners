namespace DeviceRunners.UIAutomation;

public interface IAutomatedAppCommand
{
	string Name { get; }

	object? Execute(IAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null);
}
