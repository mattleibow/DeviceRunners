namespace Xunit.Runner.Devices;

public interface ITestRunner
{
	Task<IReadOnlyList<TestAssemblyViewModel>> DiscoverAsync();

	Task RunAsync(TestCaseViewModel test);

	Task RunAsync(IEnumerable<TestCaseViewModel> tests, string? message = null);

	Task RunAsync(IReadOnlyList<AssemblyRunInfo> runInfos, string? message = null);

	event EventHandler<string> DiagnosticMessageRecieved;
}
