namespace CommunityToolkit.DeviceRunners.VisualRunners;

public class CompositeTestRunner : ITestRunner
{
	readonly IReadOnlyList<ITestRunner> _testRunners;

	public CompositeTestRunner(IEnumerable<ITestRunner> testRunners)
	{
		_testRunners = testRunners.ToList();
	}

	public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		foreach (var runner in _testRunners)
		{
			await runner.RunTestsAsync(testAssemblies, cancellationToken);
		}
	}

	public async Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		foreach (var runner in _testRunners)
		{
			await runner.RunTestsAsync(testCases, cancellationToken);
		}
	}
}
