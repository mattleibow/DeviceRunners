using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace DeviceRunners.VisualRunners.MSTest3;

public class MSTest3TestRunner : ITestRunner
{
	readonly AsyncLock _executionLock = new();

	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public MSTest3TestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
	{
		_options = options;
		_resultChannelManager = resultChannelManager;
		_diagnosticsManager = diagnosticsManager;
	}

	public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		// we can only run MSTest tests
		var grouped = testCases
			.OfType<MSTest3TestCaseInfo>()
			.GroupBy(t => t.TestAssembly)
			.Select(g => new MSTest3TestAssemblyInfo(g.Key, g.ToList()))
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
		var mstestAssemblies = testAssemblies.OfType<MSTest3TestAssemblyInfo>().ToList();
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

	void RunTests(MSTestExecutor executor, VsTestAdapterContext context, MSTest3TestAssemblyInfo assemblyInfo, CancellationToken cancellationToken)
	{
		if (assemblyInfo.TestCases.Count == 0)
			return;

		// map each VSTest TestCase.Id back to the wrapping case; DataRow rows share a fully
		// qualified name but each has a distinct Id, so match on the Id.
		var testCasesById = new Dictionary<Guid, MSTest3TestCaseInfo>();
		foreach (var testCase in assemblyInfo.TestCases)
			testCasesById[testCase.Id] = testCase;

		void OnResult(VsTestResult result)
		{
			if (!testCasesById.TryGetValue(result.TestCase.Id, out var testCase))
				return;

			var info = new MSTest3TestResultInfo(testCase, result);
			testCase.ReportResult(info);

			_resultChannelManager?.RecordResult(info);
		}

		var handle = new VsTestFrameworkHandle(OnResult, _diagnosticsManager);

		var vsTestCases = assemblyInfo.TestCases.Select(tc => tc.TestCase).ToList();

		executor.RunTests(vsTestCases, context, handle);
	}
}
