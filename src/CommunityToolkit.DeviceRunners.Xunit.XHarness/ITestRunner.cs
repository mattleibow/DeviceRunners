namespace CommunityToolkit.DeviceRunners.Xunit.XHarness;

public interface ITestRunner
{
	Task<object> RunTestsAsync();
}
