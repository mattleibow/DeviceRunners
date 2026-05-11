using NUnit.Framework.Interfaces;

namespace DeviceRunners.VisualRunners.NUnit;

class WasmNUnitTestListener : ITestListener
{
	readonly IReadOnlyDictionary<ITest, WasmNUnitTestCaseInfo> _testCases;
	readonly IResultChannel _resultChannel;

	public int Total { get; private set; }
	public int Passed { get; private set; }
	public int Failed { get; private set; }
	public int Skipped { get; private set; }

	public WasmNUnitTestListener(
		IReadOnlyDictionary<ITest, WasmNUnitTestCaseInfo> testCases,
		IResultChannel resultChannel)
	{
		_testCases = testCases;
		_resultChannel = resultChannel;
	}

	public void TestStarted(ITest test) { }

	public void TestOutput(TestOutput output) { }

	public void SendMessage(TestMessage message) { }

	public void TestFinished(ITestResult result)
	{
		// Only process leaf tests, not suites
		if (result.Test.IsSuite)
			return;

		var testCase = FindTestCase(result.Test);
		if (testCase is null)
			return;

		Total++;
		var info = new WasmNUnitTestResultInfo(testCase, result);

		switch (info.Status)
		{
			case TestResultStatus.Passed: Passed++; break;
			case TestResultStatus.Failed: Failed++; break;
			case TestResultStatus.Skipped: Skipped++; break;
		}

		testCase.ReportResult(info);
		_resultChannel.RecordResult(info);
	}

	WasmNUnitTestCaseInfo? FindTestCase(ITest test)
	{
		if (_testCases.TryGetValue(test, out var testCase))
			return testCase;

		// Fallback: match by ID
		return _testCases.Values.FirstOrDefault(tc =>
			tc.NUnitTest.Id == test.Id);
	}
}
