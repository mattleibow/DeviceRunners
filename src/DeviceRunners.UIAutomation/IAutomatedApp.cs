namespace DeviceRunners.UIAutomation;

public interface IAutomatedApp : IDisposable
{
	IAutomatedAppCommandManager Commands { get; }
}
