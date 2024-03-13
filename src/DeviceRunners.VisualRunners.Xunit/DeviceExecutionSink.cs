using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class DeviceExecutionSink : TestMessageSink
{
	readonly IReadOnlyDictionary<ITestCase, XunitTestCaseInfo> _testCases;
	readonly IResultChannelManager? _resultChannelManager;

	public DeviceExecutionSink(IReadOnlyDictionary<ITestCase, XunitTestCaseInfo> testCases, IResultChannelManager? resultChannelManager)
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
		if (!_testCases.TryGetValue(testResult.TestCase, out XunitTestCaseInfo? testCase))
		{
			// no matching reference, search by Unique ID as a fallback
			testCase = _testCases.FirstOrDefault(kvp => kvp.Key.UniqueID?.Equals(testResult.TestCase.UniqueID, StringComparison.Ordinal) ?? false).Value;

			// no tests found, so we don't know what to do
			if (testCase == null)
				return;
		}

		var result = new XunitTestResultInfo(testCase, testResult, status);
		testCase.ReportResult(result);

		_resultChannelManager?.RecordResult(result);
	}
}
