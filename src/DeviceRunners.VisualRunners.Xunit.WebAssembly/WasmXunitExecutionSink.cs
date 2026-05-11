using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class WasmXunitExecutionSink : TestMessageSink
{
	readonly IReadOnlyDictionary<ITestCase, WasmXunitTestCaseInfo> _testCases;
	readonly IResultChannel _resultChannel;

	public int Total { get; private set; }
	public int Passed { get; private set; }
	public int Failed { get; private set; }
	public int Skipped { get; private set; }

	public WasmXunitExecutionSink(
		IReadOnlyDictionary<ITestCase, WasmXunitTestCaseInfo> testCases,
		IResultChannel resultChannel)
	{
		_testCases = testCases;
		_resultChannel = resultChannel;

		Execution.TestFailedEvent += args => RecordResult(args.Message, TestResultStatus.Failed);
		Execution.TestPassedEvent += args => RecordResult(args.Message, TestResultStatus.Passed);
		Execution.TestSkippedEvent += args => RecordResult(args.Message, TestResultStatus.Skipped);
	}

	void RecordResult(ITestResultMessage testResult, TestResultStatus status)
	{
		Total++;
		switch (status)
		{
			case TestResultStatus.Passed: Passed++; break;
			case TestResultStatus.Failed: Failed++; break;
			case TestResultStatus.Skipped: Skipped++; break;
		}

		var testCase = FindTestCase(testResult.TestCase);
		if (testCase is null)
			return;

		var result = new WasmXunitTestResultInfo(testCase, testResult, status);
		testCase.ReportResult(result);
		_resultChannel.RecordResult(result);
	}

	WasmXunitTestCaseInfo? FindTestCase(ITestCase xunitTestCase)
	{
		if (_testCases.TryGetValue(xunitTestCase, out var testCase))
			return testCase;

		// Fallback: match by UniqueID
		return _testCases.Values.FirstOrDefault(tc =>
			tc.XunitTestCase.UniqueID == xunitTestCase.UniqueID);
	}
}
