using Xunit;

namespace DeviceRunners.VisualRunners.Xunit;

public class XunitTestRunner : ITestRunner
{
	readonly AsyncLock _executionLock = new();

	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public XunitTestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
	{
		_options = options;
		_resultChannelManager = resultChannelManager;
		_diagnosticsManager = diagnosticsManager;
	}

	public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		// we can only run Xunit tests
		var grouped = testCases
			.OfType<XunitTestCaseInfo>()
			.GroupBy(t => t.TestAssembly)
			.Select(g => new XunitTestAssemblyInfo(g.Key, g.ToList()))
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

	void RunTests(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		// we can only run Xunit tests
		var xunitAssemblies = testAssemblies.OfType<XunitTestAssemblyInfo>().ToList();
		if (xunitAssemblies.Count == 0)
			return;

		var pendingLocks = new List<IDisposable>();
		try
		{
			if (xunitAssemblies.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault))
			{
				// start all the tests asynchronously
				var events = xunitAssemblies
					.Select(runInfo => RunTestsAsync(runInfo, pendingLocks, cancellationToken))
					.ToList();

				// wait for them all
				events.ForEach(@event => @event.WaitOne());
			}
			else
			{
				// run the tests one by one
				foreach (var assembly in xunitAssemblies)
				{
					RunTests(assembly, pendingLocks, cancellationToken);
				}
			}
		}
		finally
		{
			pendingLocks.ForEach(disposable => disposable.Dispose());
		}
	}

	ManualResetEvent RunTestsAsync(XunitTestAssemblyInfo assemblyInfo, IList<IDisposable> pendingLocks, CancellationToken cancellationToken = default)
	{
		var mre = new ManualResetEvent(false);

		// this discard is entended as any exceptions are logged in DEBUG builds
		// and the finally ensures that the event is set
		_ = AsyncUtils.RunAsync(() =>
		{
			try
			{
				RunTests(assemblyInfo, pendingLocks, cancellationToken);
			}
			finally
			{
				mre.Set();
			}
		});

		return mre;
	}

	void RunTests(XunitTestAssemblyInfo assemblyInfo, IList<IDisposable> pendingLocks, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		var assemblyFileName = assemblyInfo.AssemblyFileName;

		var longRunningSeconds = assemblyInfo.Configuration.LongRunningTestSecondsOrDefault;

		var controller = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName);

		lock (pendingLocks)
			pendingLocks.Add(controller);

		var xunitTestCases = assemblyInfo.TestCases
			.Select(tc => new { TestCase = tc, XunitTestCase = tc.TestCase })
			.Where(tc => tc.XunitTestCase.UniqueID != null)
			.ToDictionary(tc => tc.XunitTestCase, tc => tc.TestCase);

		var executionOptions = TestFrameworkOptions.ForExecution(assemblyInfo.Configuration);

		var deviceExecSink = new DeviceExecutionSink(xunitTestCases, _resultChannelManager);

		IExecutionSink resultsSink = new DelegatingExecutionSummarySink(deviceExecSink, () => cancellationToken.IsCancellationRequested);

		if (longRunningSeconds > 0)
		{
			var diagSink = new DiagnosticMessageSink(
				d => _diagnosticsManager?.PostDiagnosticMessage(d),
				assemblyInfo.AssemblyFileName,
				executionOptions.GetDiagnosticMessagesOrDefault());

			resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagSink);
		}

		var assm = new XunitProjectAssembly { AssemblyFilename = assemblyInfo.AssemblyFileName };
		deviceExecSink.OnMessage(new TestAssemblyExecutionStarting(assm, executionOptions));

		controller.RunTests(
			xunitTestCases.Select(tc => tc.Value.TestCase).ToList(),
			resultsSink,
			executionOptions);
		resultsSink.Finished.WaitOne();

		deviceExecSink.OnMessage(new TestAssemblyExecutionFinished(assm, executionOptions, resultsSink.ExecutionSummary));
	}
}
