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

			var mstestAssemblies = testAssemblies.OfType<MSTestTestAssemblyInfo>().ToList();
			if (mstestAssemblies.Count == 0)
				return;

			// Each assembly runs in its own server-mode session, so there is no benefit to
			// overlapping them.
			foreach (var assembly in mstestAssemblies)
			{
				await RunTestsSafe(assembly, cancellationToken);
			}
		}
	}

	async Task RunTestsSafe(MSTestTestAssemblyInfo assembly, CancellationToken cancellationToken)
	{
		try
		{
			await RunTests(assembly, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Cancellation is expected — don't log as an error.
		}
		catch (Exception ex)
		{
			_diagnosticsManager?.PostDiagnosticMessage(
				$"Exception running tests in assembly '{assembly.AssemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
		}
	}

	async Task RunTests(MSTestTestAssemblyInfo assemblyInfo, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		var testCaseLookup = assemblyInfo.TestCases
			.GroupBy(tc => tc.Uid)
			.ToDictionary(g => g.Key, g => g.First());

		if (testCaseLookup.Count == 0)
			return;

		// Restrict the run to exactly the requested tests by passing their UIDs to the platform.
		var tests = testCaseLookup.Values
			.Select(tc => new MSTestServerModeHost.TestNodeRef(tc.Uid, tc.DisplayName))
			.ToList();

		void OnNode(WireTestNode node)
		{
			if (!node.IsAction)
				return;

			if (!testCaseLookup.TryGetValue(node.Uid, out var testCase))
				return;

			var result = MSTestTestResultInfo.TryCreate(testCase, node);
			if (result is null)
				return;

			testCase.ReportResult(result);
			_resultChannelManager?.RecordResult(result);
		}

		await MSTestServerModeHost.RunSessionAsync(
			assemblyInfo.Assembly,
			MSTestServerModeHost.RunTestsMethod,
			tests,
			OnNode,
			cancellationToken);
	}
}
