namespace DeviceRunners.UIAutomation;

public interface IAutomatedApp : IContainsElements
{
	IAutomatedAppCommandManager Commands { get; }
}
