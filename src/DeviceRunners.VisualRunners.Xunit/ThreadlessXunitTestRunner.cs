using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

public class ThreadlessXunitTestRunner : ITestRunner
{
	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public ThreadlessXunitTestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
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
		await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

		RunTests(testAssemblies, cancellationToken);
	}

	void RunTests(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		// we can only run Xunit tests
		var xunitAssemblies = testAssemblies.OfType<XunitTestAssemblyInfo>().ToList();
		if (xunitAssemblies.Count == 0)
			return;

		// For WASM, we run tests synchronously one by one
		foreach (var assembly in xunitAssemblies)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			RunTests(assembly, cancellationToken);
		}
	}

	void RunTests(XunitTestAssemblyInfo assemblyInfo, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		var assemblyFileName = assemblyInfo.AssemblyFileName;

		var xunitTestCases = assemblyInfo.TestCases
			.Select(tc => new { TestCase = tc, XunitTestCase = tc.TestCase })
			.Where(tc => tc.XunitTestCase.UniqueID != null)
			.ToDictionary(tc => tc.XunitTestCase, tc => tc.TestCase);

		var executionOptions = TestFrameworkOptions.ForExecution(assemblyInfo.Configuration);
		executionOptions.SetSynchronousMessageReporting(true);

		var deviceExecSink = new DeviceExecutionSink(xunitTestCases, _resultChannelManager);
		var resultsSink = new DelegatingExecutionSummarySink(deviceExecSink, () => cancellationToken.IsCancellationRequested);

		var assm = new XunitProjectAssembly { AssemblyFilename = assemblyInfo.AssemblyFileName };
		deviceExecSink.OnMessage(new TestAssemblyExecutionStarting(assm, executionOptions));

		// Use threadless execution for WASM compatibility
		var testCasesToRun = xunitTestCases.Select(tc => tc.Value.TestCase).ToList();
		
		try
		{
			var controller = new Xunit2(
				AppDomainSupport.Denied,
				new NullSourceInformationProvider(),
				assemblyFileName,
				configFileName: null,
				shadowCopy: false,
				shadowCopyFolder: null,
				diagnosticMessageSink: new ConsoleDiagnosticMessageSink(_diagnosticsManager),
				verifyTestAssemblyExists: false);

			controller.RunTests(testCasesToRun, resultsSink, executionOptions);
			resultsSink.Finished.WaitOne();
		}
		catch (Exception ex)
		{
			_diagnosticsManager?.PostDiagnosticMessage($"Exception running tests in assembly '{assemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
		}

		deviceExecSink.OnMessage(new TestAssemblyExecutionFinished(assm, executionOptions, resultsSink.ExecutionSummary));
	}
}

internal class ConsoleDiagnosticMessageSink : global::Xunit.LongLivedMarshalByRefObject, IMessageSink
{
	readonly IDiagnosticsManager? _diagnosticsManager;

	public ConsoleDiagnosticMessageSink(IDiagnosticsManager? diagnosticsManager)
	{
		_diagnosticsManager = diagnosticsManager;
	}

	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IDiagnosticMessage diagnosticMessage)
		{
			Console.WriteLine(diagnosticMessage.Message);
			_diagnosticsManager?.PostDiagnosticMessage(diagnosticMessage.Message);
		}

		return true;
	}
}