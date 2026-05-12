namespace DeviceRunners.VisualRunners;

public interface ITestResultInfo
{
	ITestCaseInfo TestCase { get; }

	/// <summary>
	/// The result status of the test.
	/// </summary>
	TestResultStatus Status { get; }

	/// <summary>
	/// The execution time of the test.
	/// </summary>
	TimeSpan Duration { get; }

	/// <summary>
	/// The captured output of the test.
	/// </summary>
	string? Output { get; }

	/// <summary>
	/// The exception messages of the test that failed.
	/// </summary>
	string? ErrorMessage { get; }

	/// <summary>
	/// The exception stack traces of the test that failed.
	/// </summary>
	string? ErrorStackTrace { get; }

	/// <summary>
	/// The reason given for the test being skipped.
	/// </summary>
	string? SkipReason { get; }
}
