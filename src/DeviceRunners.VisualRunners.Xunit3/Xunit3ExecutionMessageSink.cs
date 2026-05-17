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

	public bool OnMessage(IMessageSinkMessage message)
	{
		// Collect results to report outside the lock to avoid deadlocks
		// when subscribers (ResultReported, IResultChannel) do blocking work.
		List<(Xunit3TestCaseInfo TestCase, Xunit3TestResultInfo Result)>? resultsToReport = null;

		lock (_lock)
		{
			switch (message)
			{
				case ITestStarting testStarting:
					EnsureTestMapping(testStarting.TestUniqueID, testStarting.TestCaseUniqueID);
					break;

				case ITestPassed testPassed:
					EnsureTestMapping(testPassed.TestUniqueID, testPassed.TestCaseUniqueID);
					RecordResult(testPassed.TestUniqueID, TestResultStatus.Passed);
					break;

				case ITestFailed testFailed:
					EnsureTestMapping(testFailed.TestUniqueID, testFailed.TestCaseUniqueID);
					RecordResult(testFailed.TestUniqueID, TestResultStatus.Failed,
						errorMessage: ExceptionUtility.CombineMessages(testFailed),
						errorStackTrace: ExceptionUtility.CombineStackTraces(testFailed));
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
					resultsToReport = FlushResult(testFinished.TestUniqueID);
					break;

				// Handle framework-level errors
				case IErrorMessage errorMessage:
					_diagnosticsManager?.PostDiagnosticMessage(
						$"Framework error: {ExceptionUtility.CombineMessages(errorMessage)}");
					break;

				case ITestAssemblyCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
						$"Test assembly cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestCollectionCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
						$"Test collection cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestClassCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
						$"Test class cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestCaseCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
						$"Test case cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestCleanupFailure cleanupFailure:
					_diagnosticsManager?.PostDiagnosticMessage(
						$"Test cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestAssemblyFinished:
					resultsToReport = FlushRemainingResults();
					break;
			}
		}

		// Report results outside the lock to prevent deadlocks from subscriber callbacks.
		if (resultsToReport is not null)
		{
			foreach (var (testCase, result) in resultsToReport)
			{
				testCase.ReportResult(result);
				_resultChannelManager?.RecordResult(result);
			}
		}

		return !_cancellationToken.IsCancellationRequested;
	}

	/// <summary>
	/// Ensures a test-unique-ID → test-case-unique-ID mapping exists.
	/// Normally set by ITestStarting, but some messages (ITestNotRun,
	/// ITestSkipped, ITestPassed, ITestFailed) may fire without
	/// ITestStarting when tests are filtered or never fully started.
	/// </summary>
	void EnsureTestMapping(string testUniqueID, string testCaseUniqueID)
	{
		_testUniqueIdToTestCaseId.TryAdd(testUniqueID, testCaseUniqueID);
	}

	void RecordResult(string testUniqueID, TestResultStatus status, string? errorMessage = null, string? errorStackTrace = null, string? skipReason = null)
	{
		_pendingResults[testUniqueID] = (status, errorMessage, errorStackTrace, skipReason);
	}

	/// <summary>
	/// Tries to flush a pending result for the given test. Returns the result
	/// to report outside the lock, or null if nothing to report.
	/// Must be called while holding <see cref="_lock"/>.
	/// </summary>
	List<(Xunit3TestCaseInfo, Xunit3TestResultInfo)>? FlushResult(string testUniqueID)
	{
		if (!_pendingResults.TryGetValue(testUniqueID, out var pending))
			return null;

		_pendingResults.Remove(testUniqueID);

		if (!_testUniqueIdToTestCaseId.TryGetValue(testUniqueID, out var testCaseUniqueID))
			return null;

		if (!_testCases.TryGetValue(testCaseUniqueID, out var testCase))
			return null;

		var finishedData = _testFinishedData.GetValueOrDefault(testUniqueID);

		// Clean up tracking dictionaries now that we have all the data.
		_testFinishedData.Remove(testUniqueID);
		_testUniqueIdToTestCaseId.Remove(testUniqueID);

		var result = new Xunit3TestResultInfo(
			testCase,
			pending.Status,
			finishedData.Duration,
			output: finishedData.Output,
			errorMessage: pending.ErrorMessage,
			errorStackTrace: pending.ErrorStackTrace,
			skipReason: pending.SkipReason);

		return [(testCase, result)];
	}

	/// <summary>
	/// Flushes all remaining pending results that never received ITestFinished.
	/// Must be called while holding <see cref="_lock"/>.
	/// </summary>
	List<(Xunit3TestCaseInfo, Xunit3TestResultInfo)>? FlushRemainingResults()
	{
		List<(Xunit3TestCaseInfo, Xunit3TestResultInfo)>? all = null;

		foreach (var testUniqueID in _pendingResults.Keys.ToList())
		{
			var flushed = FlushResult(testUniqueID);
			if (flushed is not null)
			{
				all ??= [];
				all.AddRange(flushed);
			}
		}

		return all;
	}
}
