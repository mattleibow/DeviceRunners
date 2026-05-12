namespace DeviceRunners.VisualRunners.Xunit;

class WasmXunitTestResultInfo : ITestResultInfo
{
	public WasmXunitTestResultInfo(
		WasmXunitTestCaseInfo testCase,
		TestResultStatus status,
		TimeSpan duration,
		string? output,
		string? errorMessage,
		string? errorStackTrace,
		string? skipReason)
	{
		TestCase = testCase;
		Status = status;
		Duration = duration;
		Output = output;
		ErrorMessage = errorMessage;
		ErrorStackTrace = errorStackTrace;
		SkipReason = skipReason;
	}

	public WasmXunitTestCaseInfo TestCase { get; }

	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public TestResultStatus Status { get; }

	public TimeSpan Duration { get; }

	public string? Output { get; }

	public string? ErrorMessage { get; }

	public string? ErrorStackTrace { get; }

	public string? SkipReason { get; }
}
