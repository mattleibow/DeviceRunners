namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3TestResultInfo : ITestResultInfo
{
	public Xunit3TestResultInfo(Xunit3TestCaseInfo testCase, TestResultStatus status, TimeSpan duration, string? output = null, string? errorMessage = null, string? errorStackTrace = null, string? skipReason = null)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		Status = status;
		Duration = duration;
		Output = output;
		ErrorMessage = errorMessage;
		ErrorStackTrace = errorStackTrace;
		SkipReason = skipReason;
	}

	public Xunit3TestCaseInfo TestCase { get; }

	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public TimeSpan Duration { get; }

	public TestResultStatus Status { get; }

	public string? Output { get; }

	public string? ErrorMessage { get; }

	public string? ErrorStackTrace { get; }

	public string? SkipReason { get; }
}
