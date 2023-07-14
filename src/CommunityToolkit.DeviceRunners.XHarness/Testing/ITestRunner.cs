namespace CommunityToolkit.DeviceRunners.XHarness;

public interface ITestRunner
{
	Task<ITestRunResult> RunTestsAsync();
}
