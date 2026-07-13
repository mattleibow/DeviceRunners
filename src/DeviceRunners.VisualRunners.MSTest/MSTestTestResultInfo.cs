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
	public static MSTestTestResultInfo? TryCreate(MSTestTestCaseInfo testCase, WireTestNode node)
	{
		if (!node.IsTerminalResult)
			return null;

		var duration = node.DurationMs is { } ms ? TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero;

		return node.ExecutionState switch
		{
			"passed" => new MSTestTestResultInfo(testCase)
			{
				Status = TestResultStatus.Passed,
				Duration = duration,
			},
			"skipped" => new MSTestTestResultInfo(testCase)
			{
				Status = TestResultStatus.Skipped,
				Duration = duration,
				SkipReason = node.ErrorMessage,
			},
			// failed / error / timed-out / canceled all surface as a failure.
			_ => new MSTestTestResultInfo(testCase)
			{
				Status = TestResultStatus.Failed,
				Duration = duration,
				ErrorMessage = node.ErrorMessage,
				ErrorStackTrace = node.ErrorStackTrace,
			},
		};
	}
}
