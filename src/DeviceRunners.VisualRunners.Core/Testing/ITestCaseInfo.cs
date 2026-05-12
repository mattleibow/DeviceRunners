namespace DeviceRunners.VisualRunners;

public interface ITestCaseInfo
{
	ITestAssemblyInfo TestAssembly { get; }

	string DisplayName { get; }

	ITestResultInfo? Result { get; }

	event Action<ITestResultInfo>? ResultReported;
}
