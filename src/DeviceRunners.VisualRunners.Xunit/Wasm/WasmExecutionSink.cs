using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Execution message sink for WASM test runs. Hooks test result events
/// and forwards them to the visual runner infrastructure.
/// </summary>
class WasmExecutionSink : TestMessageSink
{
	readonly IReadOnlyDictionary<ITestCase, XunitWasmTestCaseInfo> _testCases;
	readonly IResultChannelManager? _resultChannelManager;

	public WasmExecutionSink(IReadOnlyDictionary<ITestCase, XunitWasmTestCaseInfo> testCases, IResultChannelManager? resultChannelManager)
	{
		_testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));
		_resultChannelManager = resultChannelManager;

		Execution.TestFailedEvent += HandleTestFailed;
		Execution.TestPassedEvent += HandleTestPassed;
		Execution.TestSkippedEvent += HandleTestSkipped;
	}

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args) =>
		RecordResult(args.Message, TestResultStatus.Failed);

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args) =>
		RecordResult(args.Message, TestResultStatus.Passed);

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args) =>
		RecordResult(args.Message, TestResultStatus.Skipped);

	void RecordResult(ITestResultMessage testResult, TestResultStatus status)
	{
		if (!_testCases.TryGetValue(testResult.TestCase, out var testCase))
		{
			// No matching reference, search by UniqueID as a fallback
			testCase = _testCases
				.FirstOrDefault(kvp => kvp.Key.UniqueID?.Equals(testResult.TestCase.UniqueID, StringComparison.Ordinal) ?? false)
				.Value;

			if (testCase is null)
				return;
		}

		var result = new XunitWasmTestResultInfo(testCase, testResult, status);
		testCase.ReportResult(result);

		_resultChannelManager?.RecordResult(result);
	}
}
