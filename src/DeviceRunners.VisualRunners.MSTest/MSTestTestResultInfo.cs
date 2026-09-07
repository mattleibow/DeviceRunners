using Microsoft.Testing.Platform.ServerMode.Client;

namespace DeviceRunners.VisualRunners.MSTest;

class MSTestTestResultInfo : ITestResultInfo
{
	MSTestTestResultInfo(MSTestTestCaseInfo testCase)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
	}

	public MSTestTestCaseInfo TestCase { get; }

	ITestCaseInfo ITestResultInfo.TestCase => TestCase;

	public TestResultStatus Status { get; private init; }

	public TimeSpan Duration { get; private init; }

	public string? Output { get; private init; }

	public string? ErrorMessage { get; private init; }

	public string? ErrorStackTrace { get; private init; }

	public string? SkipReason { get; private init; }

	/// <summary>
	/// Builds a result from a terminal server-mode node, or returns <c>null</c> when the node
	/// does not represent a final test result.
	/// </summary>
	public static MSTestTestResultInfo? TryCreate(MSTestTestCaseInfo testCase, MtpTestNodeUpdate node)
	{
		if (node.TerminalStatus() is not { } status)
			return null;

		return new MSTestTestResultInfo(testCase)
		{
			Status = status,
			Duration = node.Duration(),
			Output = node.CombinedOutput(),
			// error.message doubles as the skip reason on a skipped node.
			SkipReason = status is TestResultStatus.Skipped ? node.ErrorMessage : null,
			ErrorMessage = status is TestResultStatus.Failed ? node.ErrorMessage : null,
			ErrorStackTrace = status is TestResultStatus.Failed ? node.ErrorStackTrace : null,
		};
	}
}
