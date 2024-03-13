namespace DeviceRunners.VisualRunners;

public class CompositeTestRunner : ITestRunner
{
	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IReadOnlyList<ITestRunner> _testRunners;

	public CompositeTestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager resultChannelManager, IEnumerable<ITestRunner> testRunners)
	{
		_options = options;
		_resultChannelManager = resultChannelManager;
		_testRunners = testRunners.ToList();
	}

	public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

		foreach (var runner in _testRunners)
		{
			await runner.RunTestsAsync(testAssemblies, cancellationToken);
		}
	}

	public async Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

		foreach (var runner in _testRunners)
		{
			await runner.RunTestsAsync(testCases, cancellationToken);
		}
	}
}
