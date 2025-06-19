namespace DeviceRunners.UIAutomation;

public interface IAutomatedAppOptionsBuilder
{
	void AddCommand(IAutomatedAppCommand command);

	IAutomatedAppOptions Build();
}
