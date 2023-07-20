using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitTestResultInfo : ITestResultInfo
{
	public XunitTestResultInfo(XunitTestCaseInfo testCase, ITestResultMessage testResult, TestResultStatus status)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		TestResult = testResult ?? throw new ArgumentNullException(nameof(testResult));

		Status = status;

		Duration = TimeSpan.FromSeconds((double)testResult.ExecutionTime);

		if (testResult is ITestFailed failure)
		{
			ErrorMessage = ExceptionUtility.CombineMessages(failure);
			ErrorStackTrace = ExceptionUtility.CombineStackTraces(failure);
		}
		else if (testResult is ITestSkipped skipped)
		{
			SkipReason = skipped.Reason;
		}
	}

	public XunitTestCaseInfo TestCase { get; }
	
	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public ITestResultMessage TestResult { get; }

	public TimeSpan Duration { get; }

	public TestResultStatus Status { get; }

	public string? Output => TestResult.Output;

	public string? ErrorMessage { get; }

	public string? ErrorStackTrace { get; }

	public string? SkipReason { get; }
}
