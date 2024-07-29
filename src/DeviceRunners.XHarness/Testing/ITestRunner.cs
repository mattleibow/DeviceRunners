namespace DeviceRunners.XHarness;

public interface ITestRunner
{
	Task<ITestRunResult> RunTestsAsync();
}
