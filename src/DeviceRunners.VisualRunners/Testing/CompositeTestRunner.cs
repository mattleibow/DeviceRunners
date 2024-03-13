namespace DeviceRunners.VisualRunners;

public class CompositeTestRunner : ITestRunner
{
	readonly IVisualTestRunnerConfiguration _options;
	readonly IReadOnlyList<ITestRunner> _testRunners;

	public CompositeTestRunner(IVisualTestRunnerConfiguration options, IEnumerable<ITestRunner> testRunners)
	{
		_options = options;
		_testRunners = testRunners.ToList();
	}

	public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		await using var autoclosing = new AutoClosingResultChannel(_options.ResultChannel);
		await autoclosing.EnsureOpenAsync();

		foreach (var runner in _testRunners)
		{
			await runner.RunTestsAsync(testAssemblies, cancellationToken);
		}
	}

	public async Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		await using var autoclosing = new AutoClosingResultChannel(_options.ResultChannel);
		await autoclosing.EnsureOpenAsync();

		foreach (var runner in _testRunners)
		{
			await runner.RunTestsAsync(testCases, cancellationToken);
		}
	}
}
