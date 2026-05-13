using System.Diagnostics;
using System.Reflection;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Xunit test runner for browser WASM environments.
/// Executes tests via reflection instead of XunitFrontController.
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
		var wasmCases = testCases.OfType<XunitWasmTestCaseInfo>().ToList();
		if (wasmCases.Count == 0)
			return;

		foreach (var testCase in wasmCases)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			await RunSingleTestAsync(testCase);
		}
	}

	async Task RunSingleTestAsync(XunitWasmTestCaseInfo testCase)
	{
		if (testCase.SkipReason is not null)
		{
			var skipResult = new XunitWasmTestResultInfo(testCase, TestResultStatus.Skipped, TimeSpan.Zero, null, null, null, testCase.SkipReason);
			testCase.ReportResult(skipResult);
			_resultChannelManager?.RecordResult(skipResult);
			return;
		}

		var sw = Stopwatch.StartNew();
		try
		{
			var instance = Activator.CreateInstance(testCase.TestClass)!;

			try
			{
				var result = testCase.TestMethod.Invoke(instance, testCase.Arguments);
				if (result is Task task)
					await task;

				sw.Stop();
				var passResult = new XunitWasmTestResultInfo(testCase, TestResultStatus.Passed, sw.Elapsed, null, null, null, null);
				testCase.ReportResult(passResult);
				_resultChannelManager?.RecordResult(passResult);
			}
			finally
			{
				if (instance is IAsyncDisposable asyncDisposable)
					await asyncDisposable.DisposeAsync();
				else if (instance is IDisposable disposable)
					disposable.Dispose();
			}
		}
		catch (TargetInvocationException tie)
		{
			sw.Stop();
			var ex = tie.InnerException ?? tie;
			var failResult = new XunitWasmTestResultInfo(testCase, TestResultStatus.Failed, sw.Elapsed, null, ex.Message, ex.StackTrace, null);
			testCase.ReportResult(failResult);
			_resultChannelManager?.RecordResult(failResult);
		}
		catch (Exception ex)
		{
			sw.Stop();
			var failResult = new XunitWasmTestResultInfo(testCase, TestResultStatus.Failed, sw.Elapsed, null, ex.Message, ex.StackTrace, null);
			testCase.ReportResult(failResult);
			_resultChannelManager?.RecordResult(failResult);
		}
	}
}
