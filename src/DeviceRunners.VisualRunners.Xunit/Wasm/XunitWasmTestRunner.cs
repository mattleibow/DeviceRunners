using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Xunit test runner for browser WASM environments.
/// Uses xunit's own execution engine with cooperative yielding runners
/// for proper handling of all xunit features (fixtures, lifecycle, etc.)
/// in single-threaded WASM.
/// </summary>
public class XunitWasmTestRunner : ITestRunner
{
	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public XunitWasmTestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
	{
		_options = options;
		_resultChannelManager = resultChannelManager;
		_diagnosticsManager = diagnosticsManager;
	}

	public Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default) =>
		RunAsync(testAssemblies.SelectMany(a => a.TestCases), cancellationToken);

	public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default) =>
		RunAsync(testCases, cancellationToken);

	async Task RunAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken)
	{
		await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

		var wasmCases = testCases.OfType<XunitWasmTestCaseInfo>().ToList();
		if (wasmCases.Count == 0)
			return;

		var grouped = wasmCases.GroupBy(tc => tc.TestAssembly);

		foreach (var group in grouped)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			await RunAssemblyTestsAsync(group.Key, group.ToList(), cancellationToken);
		}
	}

	async Task RunAssemblyTestsAsync(
		XunitWasmTestAssemblyInfo assemblyInfo,
		IReadOnlyList<XunitWasmTestCaseInfo> testCases,
		CancellationToken cancellationToken)
	{
		var xunitTestCases = testCases.ToDictionary(tc => tc.TestCase, tc => tc);

		// Get the xunit ITestAssembly from the discovered test cases
		var testAssembly = testCases[0].TestCase.TestMethod.TestClass.TestCollection.TestAssembly;

		var executionOptions = TestFrameworkOptions.ForExecution(assemblyInfo.Configuration);
		executionOptions.SetSynchronousMessageReporting(true);

		var executionSink = new WasmExecutionSink(xunitTestCases, _resultChannelManager);

		try
		{
			var assemblyRunner = new WasmXunitAssemblyRunner(
				testAssembly,
				xunitTestCases.Keys.OfType<IXunitTestCase>(),
				NullMessageSink.Instance,
				executionSink,
				executionOptions);

			await assemblyRunner.RunAsync();
		}
		catch (Exception ex)
		{
			_diagnosticsManager?.PostDiagnosticMessage(
				$"Exception running tests in assembly '{assemblyInfo.AssemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
		}
	}
}
