using NUnit.Framework.Interfaces;

namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

class NUnitTestResultInfo : ITestResultInfo
{
	public NUnitTestResultInfo(NUnitTestCaseInfo testCase, ITestResult testResult)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		TestResult = testResult ?? throw new ArgumentNullException(nameof(testResult));

		Status = testResult.ResultState.Status switch
		{
			TestStatus.Skipped => TestResultStatus.Skipped,
			TestStatus.Passed => TestResultStatus.Passed,
			TestStatus.Warning => TestResultStatus.Passed,
			TestStatus.Failed => TestResultStatus.Failed,
			_ => TestResultStatus.NotRun,
		};

		Duration = TimeSpan.FromSeconds(testResult.Duration);

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

	public NUnitTestCaseInfo TestCase { get; }

	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public ITestResult TestResult { get; }

	public TimeSpan Duration { get; }

	public TestResultStatus Status { get; }

	public string? Output => TestResult.Output;

	public string? ErrorMessage { get; }

	public string? ErrorStackTrace { get; }

	public string? SkipReason { get; }
}
