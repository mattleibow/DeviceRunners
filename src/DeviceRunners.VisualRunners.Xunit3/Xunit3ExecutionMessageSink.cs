using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3ExecutionMessageSink : IMessageSink
{
	readonly IReadOnlyDictionary<string, Xunit3TestCaseInfo> _testCases;
	readonly IResultChannelManager? _resultChannelManager;
	readonly CancellationToken _cancellationToken;

	// Cache test metadata for mapping results
	readonly Dictionary<string, string> _testUniqueIdToTestCaseId = new();
	readonly Dictionary<string, (TimeSpan Duration, string? Output)> _testFinishedData = new();

	public Xunit3ExecutionMessageSink(
		IReadOnlyDictionary<string, Xunit3TestCaseInfo> testCases,
		IResultChannelManager? resultChannelManager,
		CancellationToken cancellationToken = default)
	{
		_testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));
		_resultChannelManager = resultChannelManager;
		_cancellationToken = cancellationToken;
	}

	public ManualResetEventSlim Finished { get; } = new(false);

	public bool OnMessage(IMessageSinkMessage message)
	{
		switch (message)
		{
			case ITestStarting testStarting:
				// Map test unique ID to test case unique ID for result matching
				_testUniqueIdToTestCaseId[testStarting.TestUniqueID] = testStarting.TestCaseUniqueID;
				break;

			case ITestPassed testPassed:
				RecordResult(testPassed.TestUniqueID, TestResultStatus.Passed);
				break;

			case ITestFailed testFailed:
				RecordResult(testFailed.TestUniqueID, TestResultStatus.Failed,
					errorMessage: CombineMessages(testFailed),
					errorStackTrace: CombineStackTraces(testFailed));
				break;

			case ITestSkipped testSkipped:
				RecordResult(testSkipped.TestUniqueID, TestResultStatus.Skipped,
					skipReason: testSkipped.Reason);
				break;

			case ITestNotRun testNotRun:
				RecordResult(testNotRun.TestUniqueID, TestResultStatus.NotRun);
				break;

			case ITestFinished testFinished:
				_testFinishedData[testFinished.TestUniqueID] = (
					TimeSpan.FromSeconds((double)testFinished.ExecutionTime),
					testFinished.Output);
				FlushResult(testFinished.TestUniqueID);
				break;

			case ITestAssemblyFinished:
				Finished.Set();
				break;
		}

		return !_cancellationToken.IsCancellationRequested;
	}

	// Pending results that haven't been flushed yet (waiting for ITestFinished)
	readonly Dictionary<string, (TestResultStatus Status, string? ErrorMessage, string? ErrorStackTrace, string? SkipReason)> _pendingResults = new();

	void RecordResult(string testUniqueID, TestResultStatus status, string? errorMessage = null, string? errorStackTrace = null, string? skipReason = null)
	{
		_pendingResults[testUniqueID] = (status, errorMessage, errorStackTrace, skipReason);
	}

	void FlushResult(string testUniqueID)
	{
		if (!_pendingResults.TryGetValue(testUniqueID, out var pending))
			return;

		_pendingResults.Remove(testUniqueID);

		if (!_testUniqueIdToTestCaseId.TryGetValue(testUniqueID, out var testCaseUniqueID))
			return;

		if (!_testCases.TryGetValue(testCaseUniqueID, out var testCase))
			return;

		var finishedData = _testFinishedData.GetValueOrDefault(testUniqueID);

		var result = new Xunit3TestResultInfo(
			testCase,
			pending.Status,
			finishedData.Duration,
			output: finishedData.Output,
			errorMessage: pending.ErrorMessage,
			errorStackTrace: pending.ErrorStackTrace,
			skipReason: pending.SkipReason);

		testCase.ReportResult(result);
		_resultChannelManager?.RecordResult(result);
	}

	static string? CombineMessages(ITestFailed failure)
	{
		if (failure.Messages.Length == 0)
			return null;

		return string.Join(Environment.NewLine, failure.Messages);
	}

	static string? CombineStackTraces(ITestFailed failure)
	{
		if (failure.StackTraces.Length == 0)
			return null;

		return string.Join(Environment.NewLine + "--- End of stack trace from previous exception ---" + Environment.NewLine, failure.StackTraces.Where(s => s is not null));
	}
}
