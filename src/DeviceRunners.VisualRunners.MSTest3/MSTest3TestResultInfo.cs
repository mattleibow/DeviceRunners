using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace DeviceRunners.VisualRunners.MSTest3;

class MSTest3TestResultInfo : ITestResultInfo
{
	public MSTest3TestResultInfo(MSTest3TestCaseInfo testCase, VsTestResult testResult)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		TestResult = testResult ?? throw new ArgumentNullException(nameof(testResult));

		Status = testResult.Outcome switch
		{
			TestOutcome.Passed => TestResultStatus.Passed,
			TestOutcome.Failed => TestResultStatus.Failed,
			TestOutcome.Skipped => TestResultStatus.Skipped,
			_ => TestResultStatus.NotRun,
		};

		Duration = testResult.Duration;
		Output = GetOutput(testResult);

		if (testResult.Outcome == TestOutcome.Failed)
		{
			ErrorMessage = testResult.ErrorMessage;
			ErrorStackTrace = testResult.ErrorStackTrace;
		}
		else if (testResult.Outcome == TestOutcome.Skipped)
		{
			SkipReason = testResult.ErrorMessage;
		}
	}

	public MSTest3TestCaseInfo TestCase { get; }

	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public VsTestResult TestResult { get; }

	public TimeSpan Duration { get; }

	public TestResultStatus Status { get; }

	public string? Output { get; }

	public string? ErrorMessage { get; }

	public string? ErrorStackTrace { get; }

	public string? SkipReason { get; }

	static string? GetOutput(VsTestResult testResult)
	{
		if (testResult.Messages is null || testResult.Messages.Count == 0)
			return null;

		var output = string.Concat(testResult.Messages
			.Where(m => m.Category == TestResultMessage.StandardOutCategory && m.Text is not null)
			.Select(m => m.Text));

		return string.IsNullOrEmpty(output) ? null : output;
	}
}
