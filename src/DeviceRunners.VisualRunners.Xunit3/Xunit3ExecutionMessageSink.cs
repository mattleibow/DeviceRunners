using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3ExecutionMessageSink : IMessageSink
{
	readonly object _lock = new();
	readonly IReadOnlyDictionary<string, Xunit3TestCaseInfo> _testCases;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly CancellationToken _cancellationToken;

	readonly Dictionary<string, string> _testUniqueIdToTestCaseId = new();
	readonly Dictionary<string, (TimeSpan Duration, string? Output)> _testFinishedData = new();
	readonly Dictionary<string, (TestResultStatus Status, string? ErrorMessage, string? ErrorStackTrace, string? SkipReason)> _pendingResults = new();

	public Xunit3ExecutionMessageSink(
	IReadOnlyDictionary<string, Xunit3TestCaseInfo> testCases,
	IResultChannelManager? resultChannelManager,
	IDiagnosticsManager? diagnosticsManager = null,
	CancellationToken cancellationToken = default)
	{
		_testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));
		_resultChannelManager = resultChannelManager;
		_diagnosticsManager = diagnosticsManager;
		_cancellationToken = cancellationToken;
	}

	public ManualResetEventSlim Finished { get; } = new(false);

	public bool OnMessage(IMessageSinkMessage message)
	{
		lock (_lock)
		{
			switch (message)
			{
				case ITestStarting testStarting:
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
					EnsureTestMapping(testSkipped.TestUniqueID, testSkipped.TestCaseUniqueID);
					RecordResult(testSkipped.TestUniqueID, TestResultStatus.Skipped,
						skipReason: testSkipped.Reason);
					break;

				case ITestNotRun testNotRun:
					EnsureTestMapping(testNotRun.TestUniqueID, testNotRun.TestCaseUniqueID);
					RecordResult(testNotRun.TestUniqueID, TestResultStatus.NotRun);
					break;

				case ITestFinished testFinished:
					_testFinishedData[testFinished.TestUniqueID] = (
					TimeSpan.FromSeconds((double)testFinished.ExecutionTime),
					testFinished.Output);
					FlushResult(testFinished.TestUniqueID);
					break;

				// Handle framework-level errors
				case IErrorMessage errorMessage:
					_diagnosticsManager?.PostDiagnosticMessage(
					$"Framework error: {string.Join(Environment.NewLine, errorMessage.Messages)}");
					break;

				case ITestAssemblyCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
					$"Test assembly cleanup failure: {string.Join(Environment.NewLine, cleanupFailure.Messages)}");
					break;

				case ITestCollectionCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
					$"Test collection cleanup failure: {string.Join(Environment.NewLine, cleanupFailure.Messages)}");
					break;

				case ITestClassCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
					$"Test class cleanup failure: {string.Join(Environment.NewLine, cleanupFailure.Messages)}");
					break;

				case ITestCaseCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
					$"Test case cleanup failure: {string.Join(Environment.NewLine, cleanupFailure.Messages)}");
					break;

				case ITestCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
					$"Test cleanup failure: {string.Join(Environment.NewLine, cleanupFailure.Messages)}");
					break;

				case ITestAssemblyFinished:
					// Flush any remaining pending results that never got ITestFinished
					FlushRemainingResults();
					Finished.Set();
					break;
			}
		}

		return !_cancellationToken.IsCancellationRequested;
	}

	/// <summary>
	/// Ensures a test-unique-ID → test-case-unique-ID mapping exists.
	/// Normally set by ITestStarting, but ITestNotRun and ITestSkipped
	/// may fire without ITestStarting when tests are filtered or never started.
	/// </summary>
	void EnsureTestMapping(string testUniqueID, string testCaseUniqueID)
	{
		_testUniqueIdToTestCaseId.TryAdd(testUniqueID, testCaseUniqueID);
	}

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

	void FlushRemainingResults()
	{
		foreach (var testUniqueID in _pendingResults.Keys.ToList())
		{
			FlushResult(testUniqueID);
		}
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
