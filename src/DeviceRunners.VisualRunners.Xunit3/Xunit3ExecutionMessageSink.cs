using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3ExecutionMessageSink : IMessageSink
{
	// IMessageSink.OnMessage does not guarantee single-threaded invocation.
	// With parallel test execution the message bus may dispatch from multiple
	// threads.  The lock is cheap when uncontended and keeps the internal
	// dictionaries consistent regardless of the threading model.  Result
	// and diagnostic reporting is done outside the lock to avoid deadlocks
	// if subscribers (UI, result channels, diagnostics events) do blocking work.
	readonly object _lock = new();
	readonly IReadOnlyDictionary<string, Xunit3TestCaseInfo> _testCases;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly CancellationToken _cancellationToken;

	// Per-test-case aggregation. When PreEnumerateTheories=false (the default),
	// a single test case (e.g. a [Theory]) can produce multiple ITest rows.
	// We aggregate all rows so that the worst status wins — a single failing
	// row marks the entire test case as failed.
	readonly Dictionary<string, TestCaseAggregate> _testCaseAggregates = new();

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
		// Collect results and diagnostics to report outside the lock to avoid deadlocks
		// when subscribers (ResultReported, IResultChannel, DiagnosticsManager events)
		// do blocking or UI work.
		List<(Xunit3TestCaseInfo TestCase, Xunit3TestResultInfo Result)>? resultsToReport = null;
		List<string>? diagnosticsToReport = null;

		lock (_lock)
		{
			switch (message)
			{
				case ITestPassed testPassed:
					GetOrCreateAggregate(testPassed.TestCaseUniqueID)
						.MergeResult(TestResultStatus.Passed);
					break;

				case ITestFailed testFailed:
					GetOrCreateAggregate(testFailed.TestCaseUniqueID)
						.MergeResult(TestResultStatus.Failed,
							errorMessage: ExceptionUtility.CombineMessages(testFailed),
							errorStackTrace: ExceptionUtility.CombineStackTraces(testFailed));
					break;

				case ITestSkipped testSkipped:
					GetOrCreateAggregate(testSkipped.TestCaseUniqueID)
						.MergeResult(TestResultStatus.Skipped,
							skipReason: testSkipped.Reason);
					break;

				case ITestNotRun testNotRun:
					GetOrCreateAggregate(testNotRun.TestCaseUniqueID)
						.MergeResult(TestResultStatus.NotRun);
					break;

				case ITestFinished testFinished:
					GetOrCreateAggregate(testFinished.TestCaseUniqueID)
						.AccumulateFinishedData(
							TimeSpan.FromSeconds((double)testFinished.ExecutionTime),
							testFinished.Output);
					break;

				case ITestCaseFinished testCaseFinished:
					resultsToReport = FlushTestCaseResult(testCaseFinished.TestCaseUniqueID);
					break;

				// Collect framework-level error diagnostics to report outside the lock.
				case IErrorMessage errorMessage:
					diagnosticsToReport ??= [];
					diagnosticsToReport.Add(
						$"Framework error: {ExceptionUtility.CombineMessages(errorMessage)}");
					break;

				case ITestAssemblyCleanupFailure cleanupFailure:
					diagnosticsToReport ??= [];
					diagnosticsToReport.Add(
						$"Test assembly cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestCollectionCleanupFailure cleanupFailure:
					diagnosticsToReport ??= [];
					diagnosticsToReport.Add(
						$"Test collection cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestClassCleanupFailure cleanupFailure:
					diagnosticsToReport ??= [];
					diagnosticsToReport.Add(
						$"Test class cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestCaseCleanupFailure cleanupFailure:
					diagnosticsToReport ??= [];
					diagnosticsToReport.Add(
						$"Test case cleanup failure: {ExceptionUtility.CombineMessages(cleanupFailure)}");
					break;

				case ITestCleanupFailure cleanupFailure:
					diagnosticsToReport ??= [];
					diagnosticsToReport.Add(
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

		// Report diagnostics outside the lock — event subscribers may do UI or I/O work.
		if (diagnosticsToReport is not null)
		{
			foreach (var msg in diagnosticsToReport)
			{
				_diagnosticsManager?.PostDiagnosticMessage(msg);
			}
		}

		return !_cancellationToken.IsCancellationRequested;
	}

	TestCaseAggregate GetOrCreateAggregate(string testCaseUniqueID)
	{
		if (!_testCaseAggregates.TryGetValue(testCaseUniqueID, out var aggregate))
		{
			aggregate = new TestCaseAggregate();
			_testCaseAggregates[testCaseUniqueID] = aggregate;
		}
		return aggregate;
	}

	/// <summary>
	/// Flushes the aggregated result for a test case. Called when ITestCaseFinished
	/// arrives, indicating all tests within the test case have completed.
	/// Must be called while holding <see cref="_lock"/>.
	/// </summary>
	List<(Xunit3TestCaseInfo, Xunit3TestResultInfo)>? FlushTestCaseResult(string testCaseUniqueID)
	{
		if (!_testCaseAggregates.Remove(testCaseUniqueID, out var aggregate))
			return null;

		if (!_testCases.TryGetValue(testCaseUniqueID, out var testCase))
			return null;

		var result = new Xunit3TestResultInfo(
			testCase,
			aggregate.Status ?? TestResultStatus.NotRun,
			aggregate.Duration,
			output: aggregate.Output,
			errorMessage: aggregate.ErrorMessage,
			errorStackTrace: aggregate.ErrorStackTrace,
			skipReason: aggregate.SkipReason);

		return [(testCase, result)];
	}

	/// <summary>
	/// Flushes all remaining aggregated results that never received ITestCaseFinished.
	/// Must be called while holding <see cref="_lock"/>.
	/// </summary>
	List<(Xunit3TestCaseInfo, Xunit3TestResultInfo)>? FlushRemainingResults()
	{
		List<(Xunit3TestCaseInfo, Xunit3TestResultInfo)>? all = null;

		foreach (var testCaseUniqueID in _testCaseAggregates.Keys.ToList())
		{
			var flushed = FlushTestCaseResult(testCaseUniqueID);
			if (flushed is not null)
			{
				all ??= [];
				all.AddRange(flushed);
			}
		}

		return all;
	}

	/// <summary>
	/// Accumulates per-test results for a single test case. When
	/// <c>PreEnumerateTheories</c> is <c>false</c> (the default), a [Theory]
	/// with multiple data rows produces multiple ITest results that all map
	/// to the same test case. This class aggregates them so that the worst
	/// status wins — a single failing row marks the entire test case as failed.
	/// </summary>
	class TestCaseAggregate
	{
		public TestResultStatus? Status { get; private set; }
		public TimeSpan Duration { get; private set; }
		public string? Output { get; private set; }
		public string? ErrorMessage { get; private set; }
		public string? ErrorStackTrace { get; private set; }
		public string? SkipReason { get; private set; }

		public void MergeResult(TestResultStatus status, string? errorMessage = null, string? errorStackTrace = null, string? skipReason = null)
		{
			Status = Status.HasValue ? AggregateStatus(Status.Value, status) : status;

			if (errorMessage is not null)
				ErrorMessage = ErrorMessage is null ? errorMessage : ErrorMessage + Environment.NewLine + errorMessage;

			if (errorStackTrace is not null)
				ErrorStackTrace = ErrorStackTrace is null ? errorStackTrace : ErrorStackTrace + Environment.NewLine + "---" + Environment.NewLine + errorStackTrace;

			SkipReason ??= skipReason;
		}

		public void AccumulateFinishedData(TimeSpan duration, string? output)
		{
			Duration += duration;

			if (output is not null)
				Output = Output is null ? output : Output + Environment.NewLine + output;
		}

		/// <summary>
		/// Aggregates two test result statuses. Priority: Failed wins over everything;
		/// then Passed (at least one test produced a result); then Skipped; then NotRun.
		/// </summary>
		static TestResultStatus AggregateStatus(TestResultStatus a, TestResultStatus b) =>
			(a, b) switch
			{
				_ when a == TestResultStatus.Failed || b == TestResultStatus.Failed => TestResultStatus.Failed,
				_ when a == TestResultStatus.Passed || b == TestResultStatus.Passed => TestResultStatus.Passed,
				_ when a == TestResultStatus.Skipped || b == TestResultStatus.Skipped => TestResultStatus.Skipped,
				_ => TestResultStatus.NotRun,
			};
	}
}
