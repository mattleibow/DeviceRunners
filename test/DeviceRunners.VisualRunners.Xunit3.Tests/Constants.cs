namespace VisualRunnerTests.Xunit3;

public static class Constants
{
	public const string SkippedReason = "This test is skipped.";

	public const string TestOutput = "This is test output.";

	public const string ErrorMessage = "This is meant to fail.";

	// When PreEnumerateTheories = false, [Theory] with 3 InlineData is 1 test case (not 3)
	// Xunit3 test assembly: SimpleTest + SimpleTest_Failed + SimpleTest_Skipped + DataTest (1 theory)
	//   + InitializeAsync_WasCalled + SimpleAsyncLifetimeTest
	//   + SimpleTest_Output + SimpleTest_Output_Failed = 8
	public const int Xunit3TestCountNoTheoryEnumeration = 8;
}
