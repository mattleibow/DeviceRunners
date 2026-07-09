using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace DeviceRunners.VisualRunners.MSTest;

public class MSTestTestRunner : ITestRunner
{
	readonly AsyncLock _executionLock = new();

	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public MSTestTestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
	{
		_options = options;
		_resultChannelManager = resultChannelManager;
		_diagnosticsManager = diagnosticsManager;
	}

	public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		// we can only run MSTest tests
		var grouped = testCases
			.OfType<MSTestTestCaseInfo>()
			.GroupBy(t => t.TestAssembly)
			.Select(g => new MSTestTestAssemblyInfo(g.Key, g.ToList()))
			.ToList();

		return RunTestsAsync(grouped, cancellationToken);
	}

	public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		using (await _executionLock.LockAsync())
		{
			await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

			await AsyncUtils.RunAsync(() => RunTests(testAssemblies, cancellationToken));
		}
	}

	void RunTests(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken)
	{
		// we can only run MSTest tests
		var mstestAssemblies = testAssemblies.OfType<MSTestTestAssemblyInfo>().ToList();
		if (mstestAssemblies.Count == 0)
			return;

		var context = new VsTestAdapterContext();
		var executor = new MSTestExecutor();

		// run the tests one assembly at a time
		foreach (var assembly in mstestAssemblies)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			RunTests(executor, context, assembly, cancellationToken);
		}
	}

	void RunTests(MSTestExecutor executor, VsTestAdapterContext context, MSTestTestAssemblyInfo assemblyInfo, CancellationToken cancellationToken)
	{
		if (assemblyInfo.TestCases.Count == 0)
			return;

		// map each VSTest TestCase.Id back to the wrapping case; DataRow rows share a fully
		// qualified name but each has a distinct Id, so match on the Id.
		var testCasesById = new Dictionary<Guid, MSTestTestCaseInfo>();
		foreach (var testCase in assemblyInfo.TestCases)
			testCasesById[testCase.Id] = testCase;

		void OnResult(VsTestResult result)
		{
			if (!testCasesById.TryGetValue(result.TestCase.Id, out var testCase))
				return;

			var info = new MSTestTestResultInfo(testCase, result);
			testCase.ReportResult(info);

			_resultChannelManager?.RecordResult(info);
		}

		var handle = new VsTestFrameworkHandle(OnResult, _diagnosticsManager);

		var vsTestCases = assemblyInfo.TestCases.Select(tc => tc.TestCase).ToList();

		executor.RunTests(vsTestCases, context, handle);
	}
}
