namespace Xunit.Runner.Devices.XHarness;

public interface ITestRunner
{
	Task<object> RunTestsAsync();
}
