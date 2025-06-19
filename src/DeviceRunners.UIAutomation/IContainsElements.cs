namespace DeviceRunners.UIAutomation;

public interface IContainsElements
{
	IAutomatedAppElement FindElement(Action<IBy> by);

	IReadOnlyList<IAutomatedAppElement> FindElements(Action<IBy> by);
}
