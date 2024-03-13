using Microsoft.Extensions.Logging;

using NUnit;
using NUnit.Framework.Api;
using NUnit.Framework.Internal.Builders;

namespace DeviceRunners.VisualRunners.NUnit;

public class NUnitTestRunner : ITestRunner
{
	readonly AsyncLock _executionLock = new();
	readonly IDiagnosticsManager? _diagnosticsManager;

	public NUnitTestRunner(IDiagnosticsManager? diagnosticsManager = null, ILogger<NUnitTestDiscoverer>? logger = null)
	{
		_diagnosticsManager = diagnosticsManager;
	}

	public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		// we can only run NUnit tests
		var grouped = testCases
			.OfType<NUnitTestCaseInfo>()
			.GroupBy(t => t.TestAssembly)
			.Select(g => new NUnitTestAssemblyInfo(g.Key, g.ToList()))
			.ToList();

		return RunTestsAsync(grouped, cancellationToken);
	}

	public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		using (await _executionLock.LockAsync())
		{
			// message ??= runInfos.Count > 1 || runInfos.FirstOrDefault()?.TestCases.Count > 1
			// 	? "Run Multiple Tests"
			// 	: runInfos.FirstOrDefault()?.TestCases.FirstOrDefault()?.DisplayName;

			// _logger.LogTestStart(message);

			try
			{
				await AsyncUtils.RunAsync(() => RunTests(testAssemblies, cancellationToken));
			}
			finally
			{
				// _logger.LogTestComplete();
			}
		}
	}

	void RunTests(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		// we can only run NUnit tests
		var nunitAssemblies = testAssemblies.OfType<NUnitTestAssemblyInfo>().ToList();
		if (nunitAssemblies.Count == 0)
			return;

		var pendingLocks = new List<IDisposable>();
		try
		{
			// TODO: parallel
			// if (nunitAssemblies.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault))
			// {
			// 	// start all the tests asynchronously
			// 	var events = nunitAssemblies
			// 		.Select(runInfo => RunTestsAsync(runInfo, pendingLocks, cancellationToken))
			// 		.ToList();

			// 	// wait for them all
			// 	events.ForEach(@event => @event.WaitOne());
			// }
			// else
			{
				// run the tests one by one
				foreach (var assembly in nunitAssemblies)
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

	ManualResetEvent RunTestsAsync(NUnitTestAssemblyInfo assemblyInfo, IList<IDisposable> pendingLocks, CancellationToken cancellationToken = default)
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

	void RunTests(NUnitTestAssemblyInfo assemblyInfo, IList<IDisposable> pendingLocks, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		// var assemblyFileName = assemblyInfo.AssemblyFileName;

		// var longRunningSeconds = assemblyInfo.Configuration.LongRunningTestSecondsOrDefault;

		// var controller = new NUnitFrontController(AppDomainSupport.Denied, assemblyFileName);

		// lock (pendingLocks)
		// 	pendingLocks.Add(controller);

		var nunitTestCases = assemblyInfo.TestCases
			.Select(tc => new { TestCase = tc, NUnitTestCase = tc.TestCase })
			.ToDictionary(tc => tc.NUnitTestCase, tc => tc.TestCase);

		// var executionOptions = TestFrameworkOptions.ForExecution(assemblyInfo.Configuration);

		var listener = new NUnitTestListener(
			nunitTestCases);//,
			// d => _diagnosticsManager?.PostDiagnosticMessage(d),
			// assemblyInfo.AssemblyFileName,
			// true);

		// IExecutionSink resultsSink = new DelegatingExecutionSummarySink(deviceExecSink, () => cancellationToken.IsCancellationRequested);

		// if (longRunningSeconds > 0)
		// {
		// 	var diagSink = new DiagnosticMessageSink(
		// 		d => _diagnosticsManager?.PostDiagnosticMessage(d),
		// 		assemblyInfo.AssemblyFileName,
		// 		executionOptions.GetDiagnosticMessagesOrDefault());

		// 	resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagSink);
		// }

		// var assm = new NUnitProjectAssembly { AssemblyFilename = assemblyInfo.AssemblyFileName };
		// deviceExecSink.OnMessage(new TestAssemblyExecutionStarting(assm, executionOptions));

		var builder = new NUnitTestAssemblyInfoBuilder(assemblyInfo);
		var runner = new NUnitTestAssemblyRunner(builder);

		runner.Load(assemblyInfo.AssemblyFileName, assemblyInfo.Configuration);

		var result = runner.Run(listener, new TestCaseTestFilter(assemblyInfo.TestCases));

		// controller.RunTests(
		// 	NUnitTestCases.Select(tc => tc.Value.TestCase).ToList(),
		// 	resultsSink,
		// 	executionOptions);
		// resultsSink.Finished.WaitOne();

		// deviceExecSink.OnMessage(new TestAssemblyExecutionFinished(assm, executionOptions, resultsSink.ExecutionSummary));
	}
}
