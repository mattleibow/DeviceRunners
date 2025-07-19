namespace DeviceRunners.UIAutomation;

public interface IAutomationFramework : IDisposable
{
	IReadOnlyList<IAutomatedAppOptions> AvailableApps { get; }

	IAutomatedApp CreateApp(IAutomatedAppOptions options);

	void StartApp(IAutomatedApp app);

	void StopApp(IAutomatedApp app);

	void RestartApp(IAutomatedApp app);
}
