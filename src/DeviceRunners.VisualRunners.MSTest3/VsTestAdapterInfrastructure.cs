using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace DeviceRunners.VisualRunners.MSTest3;

/// <summary>
/// Minimal <see cref="IDiscoveryContext"/>/<see cref="IRunContext"/> implementation used to drive
/// the MSTest VSTest adapter in-process. No run settings or filtering is applied here; the set of
/// tests to run is controlled by passing an explicit list of <see cref="TestCase"/>s to the executor.
/// </summary>
class VsTestAdapterContext : IDiscoveryContext, IRunContext
{
	public IRunSettings? RunSettings => null;

	public bool KeepAlive => false;

	public bool InIsolation => false;

	public bool IsDataCollectionEnabled => false;

	public bool IsBeingDebugged => false;

	public string? TestRunDirectory => null;

	public string? SolutionDirectory => null;

	public ITestCaseFilterExpression? GetTestCaseFilter(IEnumerable<string>? supportedProperties, Func<string, TestProperty?> propertyProvider) => null;
}

/// <summary>
/// Forwards adapter diagnostic messages to the optional <see cref="IDiagnosticsManager"/>.
/// </summary>
class VsTestMessageLogger : IMessageLogger
{
	readonly IDiagnosticsManager? _diagnosticsManager;

	public VsTestMessageLogger(IDiagnosticsManager? diagnosticsManager)
	{
		_diagnosticsManager = diagnosticsManager;
	}

	public void SendMessage(TestMessageLevel testMessageLevel, string message)
	{
		if (testMessageLevel >= TestMessageLevel.Warning)
			_diagnosticsManager?.PostDiagnosticMessage($"[MSTest {testMessageLevel}] {message}");
	}
}

/// <summary>
/// Collects the <see cref="TestCase"/>s reported during discovery.
/// </summary>
class VsTestDiscoverySink : ITestCaseDiscoverySink
{
	public List<TestCase> TestCases { get; } = new();

	public void SendTestCase(TestCase discoveredTest) => TestCases.Add(discoveredTest);
}

/// <summary>
/// Receives per-test results from the MSTest executor and forwards each one to a callback.
/// </summary>
class VsTestFrameworkHandle : IFrameworkHandle
{
	readonly Action<VsTestResult> _onResult;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public VsTestFrameworkHandle(Action<VsTestResult> onResult, IDiagnosticsManager? diagnosticsManager)
	{
		_onResult = onResult ?? throw new ArgumentNullException(nameof(onResult));
		_diagnosticsManager = diagnosticsManager;
	}

	public bool EnableShutdownAfterTestRun { get; set; }

	public void RecordResult(VsTestResult testResult) => _onResult(testResult);

	public void RecordStart(TestCase testCase) { }

	public void RecordEnd(TestCase testCase, TestOutcome outcome) { }

	public void RecordAttachments(IList<AttachmentSet> attachmentSets) { }

	public void SendMessage(TestMessageLevel testMessageLevel, string message)
	{
		if (testMessageLevel >= TestMessageLevel.Warning)
			_diagnosticsManager?.PostDiagnosticMessage($"[MSTest {testMessageLevel}] {message}");
	}

	public int LaunchProcessWithDebuggerAttached(string filePath, string? workingDirectory, string? arguments, IDictionary<string, string?>? environmentVariables) =>
		throw new NotSupportedException();
}
