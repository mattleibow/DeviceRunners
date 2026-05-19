namespace VisualRunnerTests;

public static class Constants
{
	public const string SkippedReason = "This test is skipped.";

	public const string TestOutput = "This is test output.";

	public const string ErrorMessage = "This is meant to fail.";

	public const int TestCount = 8;

	// When PreEnumerateTheories = false, [Theory] with 3 InlineData is 1 test case (not 3)
	public const int TestCountNoTheoryEnumeration = 6;
}
