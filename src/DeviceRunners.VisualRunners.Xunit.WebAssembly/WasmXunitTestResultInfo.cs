using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

class WasmXunitTestResultInfo : ITestResultInfo
{
	public WasmXunitTestResultInfo(WasmXunitTestCaseInfo testCase, ITestResultMessage testResult, TestResultStatus status)
	{
		TestCase = testCase;
		Status = status;
		Duration = TimeSpan.FromSeconds((double)testResult.ExecutionTime);
		Output = testResult.Output;

		if (testResult is ITestFailed failure)
		{
			ErrorMessage = ExceptionUtility.CombineMessages(failure);
			ErrorStackTrace = ExceptionUtility.CombineStackTraces(failure);
		}
		else if (testResult is ITestSkipped skippedResult)
		{
			SkipReason = skippedResult.Reason;
		}
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
