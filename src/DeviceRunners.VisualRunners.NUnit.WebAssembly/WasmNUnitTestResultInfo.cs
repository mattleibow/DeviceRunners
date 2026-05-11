using NUnit.Framework.Interfaces;

namespace DeviceRunners.VisualRunners.NUnit;

class WasmNUnitTestResultInfo : ITestResultInfo
{
	public WasmNUnitTestResultInfo(WasmNUnitTestCaseInfo testCase, ITestResult testResult)
	{
		TestCase = testCase;

		Status = testResult.ResultState.Status switch
		{
			TestStatus.Passed => TestResultStatus.Passed,
			TestStatus.Warning => TestResultStatus.Passed,
			TestStatus.Failed => TestResultStatus.Failed,
			TestStatus.Skipped => TestResultStatus.Skipped,
			_ => TestResultStatus.NotRun,
		};

		Duration = TimeSpan.FromSeconds(testResult.Duration);
		Output = testResult.Output;

		if (testResult.ResultState.Status == TestStatus.Failed)
		{
			ErrorMessage = testResult.Message;
			ErrorStackTrace = testResult.StackTrace;
		}
		else if (testResult.ResultState.Status == TestStatus.Skipped)
		{
			SkipReason = testResult.Message;
		}
	}

	public WasmNUnitTestCaseInfo TestCase { get; }

	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public TestResultStatus Status { get; }

	public TimeSpan Duration { get; }

	public string? Output { get; }

	public string? ErrorMessage { get; }

	public string? ErrorStackTrace { get; }

	public string? SkipReason { get; }
}
