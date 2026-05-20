using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using NSubstitute;

using Xunit;
using Xunit.Sdk;

namespace VisualRunnerTests.Xunit3.Testing;

public class Xunit3ExecutionMessageSinkTests
{
	const string TestCaseId1 = "test-case-1";
	const string TestCaseId2 = "test-case-2";
	const string TestCaseId3 = "test-case-3";

	static Xunit3TestCaseInfo CreateTestCaseInfo(string uniqueId, string displayName = "Test")
	{
		var testCase = Substitute.For<ITestCase>();
		testCase.UniqueID.Returns(uniqueId);
		testCase.TestCaseDisplayName.Returns(displayName);
		testCase.TestClassName.Returns("TestClass");
		testCase.TestMethodName.Returns("TestMethod");

		var assembly = new Xunit3TestAssemblyInfo("TestAssembly.dll");
		return new Xunit3TestCaseInfo(assembly, testCase);
	}

	static IReadOnlyDictionary<string, Xunit3TestCaseInfo> CreateTestCaseDictionary(params Xunit3TestCaseInfo[] testCases) =>
		testCases.ToDictionary(tc => tc.TestCaseUniqueID);

	static ITestPassed CreateTestPassed(string testCaseUniqueId)
	{
		var msg = Substitute.For<ITestPassed>();
		msg.TestCaseUniqueID.Returns(testCaseUniqueId);
		return msg;
	}

	static ITestFailed CreateTestFailed(string testCaseUniqueId, string errorMessage = "Test failed", string stackTrace = "at Test.Method()")
	{
		var msg = Substitute.For<ITestFailed>();
		msg.TestCaseUniqueID.Returns(testCaseUniqueId);
		msg.Messages.Returns([errorMessage]);
		msg.StackTraces.Returns([stackTrace]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);
		return msg;
	}

	static ITestSkipped CreateTestSkipped(string testCaseUniqueId, string reason = "Skipped")
	{
		var msg = Substitute.For<ITestSkipped>();
		msg.TestCaseUniqueID.Returns(testCaseUniqueId);
		msg.Reason.Returns(reason);
		return msg;
	}

	static ITestNotRun CreateTestNotRun(string testCaseUniqueId)
	{
		var msg = Substitute.For<ITestNotRun>();
		msg.TestCaseUniqueID.Returns(testCaseUniqueId);
		return msg;
	}

	static ITestFinished CreateTestFinished(string testCaseUniqueId, decimal executionTime = 0.5m, string? output = null)
	{
		var msg = Substitute.For<ITestFinished>();
		msg.TestCaseUniqueID.Returns(testCaseUniqueId);
		msg.ExecutionTime.Returns(executionTime);
		msg.Output.Returns(output ?? string.Empty);
		return msg;
	}

	static ITestCaseFinished CreateTestCaseFinished(string testCaseUniqueId)
	{
		var msg = Substitute.For<ITestCaseFinished>();
		msg.TestCaseUniqueID.Returns(testCaseUniqueId);
		return msg;
	}

	static ITestAssemblyFinished CreateTestAssemblyFinished()
	{
		return Substitute.For<ITestAssemblyFinished>();
	}

	#region Theory Aggregation Tests

	[Fact]
	public void AggregatesTheoryRows_FailedWinsOverPassed()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1, "DataTest");
		var testCases = CreateTestCaseDictionary(testCase);
		var resultChannel = Substitute.For<IResultChannelManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, resultChannel);

		// Simulate theory with 3 rows: pass, pass, fail
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m, "row1 output"));
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.2m, "row2 output"));
		sink.OnMessage(CreateTestFailed(TestCaseId1, "Row 3 failed", "at Row3()"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.3m, "row3 output"));

		// Flush on ITestCaseFinished
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Failed, testCase.Result.Status);
		Assert.Contains("Row 3 failed", testCase.Result.ErrorMessage);
		Assert.Contains("at Row3()", testCase.Result.ErrorStackTrace);
	}

	[Fact]
	public void AggregatesTheoryRows_PassedWinsOverSkipped()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1, "DataTest");
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestSkipped(TestCaseId1, "Some reason"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m));
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.2m));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Passed, testCase.Result.Status);
	}

	[Fact]
	public void AggregatesTheoryRows_SkippedWinsOverNotRun()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestNotRun(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.0m));
		sink.OnMessage(CreateTestSkipped(TestCaseId1, "Skipped reason"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.0m));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Skipped, testCase.Result.Status);
		Assert.Equal("Skipped reason", testCase.Result.SkipReason);
	}

	[Fact]
	public void AggregatesDurationAndOutputAcrossTheoryRows()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1, "DataTest");
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.5m, "row1"));
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.3m, "row2"));
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.2m, "row3"));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TimeSpan.FromSeconds(1.0), testCase.Result.Duration);
		Assert.Contains("row1", testCase.Result.Output);
		Assert.Contains("row2", testCase.Result.Output);
		Assert.Contains("row3", testCase.Result.Output);
	}

	[Fact]
	public void AggregatesMultipleFailures_ConcatenatesErrorMessagesAndStackTraces()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestFailed(TestCaseId1, "Error 1", "Stack 1"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m));
		sink.OnMessage(CreateTestFailed(TestCaseId1, "Error 2", "Stack 2"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Failed, testCase.Result.Status);
		Assert.Contains("Error 1", testCase.Result.ErrorMessage);
		Assert.Contains("Error 2", testCase.Result.ErrorMessage);
		Assert.Contains("Stack 1", testCase.Result.ErrorStackTrace);
		Assert.Contains("Stack 2", testCase.Result.ErrorStackTrace);
		Assert.Contains("---", testCase.Result.ErrorStackTrace);
	}

	#endregion

	#region Flush Behavior Tests

	[Fact]
	public void FlushesResultOnTestCaseFinished()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);
		var resultChannel = Substitute.For<IResultChannelManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, resultChannel);

		Xunit3TestResultInfo? reportedResult = null;
		testCase.ResultReported += r => reportedResult = (Xunit3TestResultInfo)r;

		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.5m));

		// Before ITestCaseFinished — no result yet
		Assert.Null(reportedResult);

		// After ITestCaseFinished — result flushed
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(reportedResult);
		Assert.Equal(TestResultStatus.Passed, reportedResult.Status);
		resultChannel.Received(1).RecordResult(Arg.Any<ITestResultInfo>());
	}

	[Fact]
	public void FlushesOrphanResultsOnAssemblyFinished()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);
		var resultChannel = Substitute.For<IResultChannelManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, resultChannel);

		// Send pass + finished but NO ITestCaseFinished
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.5m, "output"));

		Assert.Null(testCase.Result);

		// ITestAssemblyFinished should flush remaining
		sink.OnMessage(CreateTestAssemblyFinished());

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Passed, testCase.Result.Status);
		Assert.Equal("output", testCase.Result.Output);
		resultChannel.Received(1).RecordResult(Arg.Any<ITestResultInfo>());
	}

	[Fact]
	public void FlushesMultipleTestCasesOnAssemblyFinished()
	{
		var testCase1 = CreateTestCaseInfo(TestCaseId1, "Test1");
		var testCase2 = CreateTestCaseInfo(TestCaseId2, "Test2");
		var testCases = CreateTestCaseDictionary(testCase1, testCase2);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m));
		sink.OnMessage(CreateTestFailed(TestCaseId2, "fail"));
		sink.OnMessage(CreateTestFinished(TestCaseId2, 0.2m));

		// Flush all at once
		sink.OnMessage(CreateTestAssemblyFinished());

		Assert.NotNull(testCase1.Result);
		Assert.Equal(TestResultStatus.Passed, testCase1.Result.Status);
		Assert.NotNull(testCase2.Result);
		Assert.Equal(TestResultStatus.Failed, testCase2.Result.Status);
	}

	#endregion

	#region Cleanup Failure Routing Tests

	[Fact]
	public void RoutesTestAssemblyCleanupFailureToDiagnostics()
	{
		var testCases = CreateTestCaseDictionary();
		var diagnostics = Substitute.For<IDiagnosticsManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, diagnostics);

		var msg = Substitute.For<ITestAssemblyCleanupFailure>();
		msg.Messages.Returns(["Cleanup failed"]);
		msg.StackTraces.Returns(["at Cleanup()"]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);

		sink.OnMessage(msg);

		diagnostics.Received(1).PostDiagnosticMessage(
			Arg.Is<string>(s => s.Contains("Test assembly cleanup failure")));
	}

	[Fact]
	public void RoutesTestCollectionCleanupFailureToDiagnostics()
	{
		var testCases = CreateTestCaseDictionary();
		var diagnostics = Substitute.For<IDiagnosticsManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, diagnostics);

		var msg = Substitute.For<ITestCollectionCleanupFailure>();
		msg.Messages.Returns(["Collection cleanup failed"]);
		msg.StackTraces.Returns(["at Collection()"]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);

		sink.OnMessage(msg);

		diagnostics.Received(1).PostDiagnosticMessage(
			Arg.Is<string>(s => s.Contains("Test collection cleanup failure")));
	}

	[Fact]
	public void RoutesTestClassCleanupFailureToDiagnostics()
	{
		var testCases = CreateTestCaseDictionary();
		var diagnostics = Substitute.For<IDiagnosticsManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, diagnostics);

		var msg = Substitute.For<ITestClassCleanupFailure>();
		msg.Messages.Returns(["Class cleanup failed"]);
		msg.StackTraces.Returns(["at Class()"]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);

		sink.OnMessage(msg);

		diagnostics.Received(1).PostDiagnosticMessage(
			Arg.Is<string>(s => s.Contains("Test class cleanup failure")));
	}

	[Fact]
	public void RoutesTestCaseCleanupFailureToDiagnostics()
	{
		var testCases = CreateTestCaseDictionary();
		var diagnostics = Substitute.For<IDiagnosticsManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, diagnostics);

		var msg = Substitute.For<ITestCaseCleanupFailure>();
		msg.Messages.Returns(["Case cleanup failed"]);
		msg.StackTraces.Returns(["at Case()"]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);

		sink.OnMessage(msg);

		diagnostics.Received(1).PostDiagnosticMessage(
			Arg.Is<string>(s => s.Contains("Test case cleanup failure")));
	}

	[Fact]
	public void RoutesTestCleanupFailureToDiagnostics()
	{
		var testCases = CreateTestCaseDictionary();
		var diagnostics = Substitute.For<IDiagnosticsManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, diagnostics);

		var msg = Substitute.For<ITestCleanupFailure>();
		msg.Messages.Returns(["Test cleanup failed"]);
		msg.StackTraces.Returns(["at Test()"]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);

		sink.OnMessage(msg);

		diagnostics.Received(1).PostDiagnosticMessage(
			Arg.Is<string>(s => s.Contains("Test cleanup failure")));
	}

	[Fact]
	public void RoutesErrorMessageToDiagnostics()
	{
		var testCases = CreateTestCaseDictionary();
		var diagnostics = Substitute.For<IDiagnosticsManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, diagnostics);

		var msg = Substitute.For<IErrorMessage>();
		msg.Messages.Returns(["Framework error occurred"]);
		msg.StackTraces.Returns(["at Framework()"]);
		msg.ExceptionTypes.Returns(["System.InvalidOperationException"]);
		msg.ExceptionParentIndices.Returns([-1]);

		sink.OnMessage(msg);

		diagnostics.Received(1).PostDiagnosticMessage(
			Arg.Is<string>(s => s.Contains("Framework error")));
	}

	#endregion

	#region Output and Error Capture Tests

	[Fact]
	public void FailedTestWithOutput_PreservesBothOutputAndError()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestFailed(TestCaseId1, "Assertion failed", "at Assert()"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.5m, "Test wrote this output"));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Failed, testCase.Result.Status);
		Assert.Equal("Test wrote this output", testCase.Result.Output);
		Assert.Contains("Assertion failed", testCase.Result.ErrorMessage);
		Assert.Contains("at Assert()", testCase.Result.ErrorStackTrace);
	}

	[Fact]
	public void PassedTestWithOutput_CapturesOutput()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.3m, "Some output"));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Passed, testCase.Result.Status);
		Assert.Equal("Some output", testCase.Result.Output);
	}

	[Fact]
	public void SkippedTest_CapturesSkipReason()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestSkipped(TestCaseId1, "Not supported on this platform"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.0m));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Skipped, testCase.Result.Status);
		Assert.Equal("Not supported on this platform", testCase.Result.SkipReason);
	}

	[Fact]
	public void SkipReason_FirstWins_WhenMultipleSkips()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestSkipped(TestCaseId1, "First reason"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.0m));
		sink.OnMessage(CreateTestSkipped(TestCaseId1, "Second reason"));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.0m));

		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal("First reason", testCase.Result.SkipReason);
	}

	#endregion

	#region Cancellation Tests

	[Fact]
	public void CancelledToken_ReturnsFalseFromOnMessage()
	{
		var testCases = CreateTestCaseDictionary();
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, cancellationToken: cts.Token);

		var result = sink.OnMessage(CreateTestAssemblyFinished());

		Assert.False(result);
	}

	[Fact]
	public void NotCancelledToken_ReturnsTrueFromOnMessage()
	{
		var testCases = CreateTestCaseDictionary();

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		var result = sink.OnMessage(CreateTestAssemblyFinished());

		Assert.True(result);
	}

	#endregion

	#region Unknown Test Case Handling

	[Fact]
	public void UnknownTestCaseId_IsIgnoredGracefully()
	{
		var testCases = CreateTestCaseDictionary(); // empty dictionary
		var resultChannel = Substitute.For<IResultChannelManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, resultChannel);

		// Should not throw
		sink.OnMessage(CreateTestPassed("unknown-id"));
		sink.OnMessage(CreateTestFinished("unknown-id", 0.1m));
		sink.OnMessage(CreateTestCaseFinished("unknown-id"));

		resultChannel.DidNotReceive().RecordResult(Arg.Any<ITestResultInfo>());
	}

	[Fact]
	public void UnknownTestCaseId_DoesNotPreventOtherResults()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		// Unknown case
		sink.OnMessage(CreateTestPassed("unknown-id"));
		sink.OnMessage(CreateTestFinished("unknown-id", 0.1m));
		sink.OnMessage(CreateTestCaseFinished("unknown-id"));

		// Known case
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.2m));
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Passed, testCase.Result.Status);
	}

	#endregion

	#region Concurrent Message Delivery Tests

	[Fact]
	public async Task ConcurrentOnMessage_IsThreadSafe()
	{
		const int threadCount = 8;
		var testCases = new List<Xunit3TestCaseInfo>();
		for (int i = 0; i < threadCount; i++)
		{
			testCases.Add(CreateTestCaseInfo($"tc-{i}", $"Test {i}"));
		}
		var dict = testCases.ToDictionary(tc => tc.TestCaseUniqueID);
		var resultChannel = Substitute.For<IResultChannelManager>();

		var sink = new Xunit3ExecutionMessageSink(dict, resultChannel);
		var reportedCount = 0;

		foreach (var tc in testCases)
		{
			tc.ResultReported += _ => Interlocked.Increment(ref reportedCount);
		}

		// Spin up threads that each process their own test case concurrently
		var tasks = testCases.Select(tc => Task.Run(() =>
		{
			var id = tc.TestCaseUniqueID;
			sink.OnMessage(CreateTestPassed(id));
			sink.OnMessage(CreateTestFinished(id, 0.1m, $"output-{id}"));
			sink.OnMessage(CreateTestCaseFinished(id));
		})).ToArray();

		await Task.WhenAll(tasks);

		// All test cases should have results
		foreach (var tc in testCases)
		{
			Assert.NotNull(tc.Result);
			Assert.Equal(TestResultStatus.Passed, tc.Result.Status);
		}
		Assert.Equal(threadCount, reportedCount);
	}

	[Fact]
	public async Task ConcurrentOnMessage_SameTestCase_AggregatesCorrectly()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1, "ParallelTheory");
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		// Multiple threads sending messages for the same test case (theory rows)
		var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
		{
			if (i == 5)
				sink.OnMessage(CreateTestFailed(TestCaseId1, $"Error {i}", $"Stack {i}"));
			else
				sink.OnMessage(CreateTestPassed(TestCaseId1));
			sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m, $"output-{i}"));
		})).ToArray();

		await Task.WhenAll(tasks);
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
		Assert.Equal(TestResultStatus.Failed, testCase.Result.Status);
		Assert.Equal(TimeSpan.FromSeconds(1.0), testCase.Result.Duration);
	}

	#endregion

	#region ResultReported Event Tests

	[Fact]
	public void ReportResult_FiresResultReportedEvent()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);

		ITestResultInfo? reported = null;
		testCase.ResultReported += r => reported = r;

		var result = new Xunit3TestResultInfo(
			testCase, TestResultStatus.Passed, TimeSpan.FromSeconds(1));

		testCase.ReportResult(result);

		Assert.NotNull(reported);
		Assert.Same(result, reported);
		Assert.Same(result, testCase.Result);
	}

	[Fact]
	public void ReportResult_MultipleSubscribers_AllFire()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);

		int count = 0;
		testCase.ResultReported += _ => Interlocked.Increment(ref count);
		testCase.ResultReported += _ => Interlocked.Increment(ref count);
		testCase.ResultReported += _ => Interlocked.Increment(ref count);

		var result = new Xunit3TestResultInfo(
			testCase, TestResultStatus.Passed, TimeSpan.FromSeconds(1));

		testCase.ReportResult(result);

		Assert.Equal(3, count);
	}

	[Fact]
	public void ReportResult_CalledTwice_OverwritesPreviousResult()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);

		int count = 0;
		testCase.ResultReported += _ => count++;

		var result1 = new Xunit3TestResultInfo(
			testCase, TestResultStatus.Failed, TimeSpan.FromSeconds(1), errorMessage: "First run");
		var result2 = new Xunit3TestResultInfo(
			testCase, TestResultStatus.Passed, TimeSpan.FromSeconds(0.5));

		testCase.ReportResult(result1);
		testCase.ReportResult(result2);

		Assert.Same(result2, testCase.Result);
		Assert.Equal(2, count);
	}

	#endregion

	#region Result Channel Integration

	[Fact]
	public void ResultChannel_ReceivesResultOnFlush()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);
		var resultChannel = Substitute.For<IResultChannelManager>();

		var sink = new Xunit3ExecutionMessageSink(testCases, resultChannel);

		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.5m));
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		resultChannel.Received(1).RecordResult(Arg.Is<ITestResultInfo>(r =>
			r.Status == TestResultStatus.Passed));
	}

	[Fact]
	public void NullResultChannel_DoesNotThrow()
	{
		var testCase = CreateTestCaseInfo(TestCaseId1);
		var testCases = CreateTestCaseDictionary(testCase);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.5m));
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		Assert.NotNull(testCase.Result);
	}

	[Fact]
	public void NullDiagnosticsManager_DoesNotThrow()
	{
		var testCases = CreateTestCaseDictionary();

		var sink = new Xunit3ExecutionMessageSink(testCases, null, null);

		var msg = Substitute.For<IErrorMessage>();
		msg.Messages.Returns(["Error"]);
		msg.StackTraces.Returns(["Stack"]);
		msg.ExceptionTypes.Returns(["System.Exception"]);
		msg.ExceptionParentIndices.Returns([-1]);

		// Should not throw even with null diagnostics
		sink.OnMessage(msg);
	}

	#endregion

	#region Multiple Test Cases in Single Assembly

	[Fact]
	public void MultipleTestCases_EachReportsIndependently()
	{
		var tc1 = CreateTestCaseInfo(TestCaseId1, "Test1");
		var tc2 = CreateTestCaseInfo(TestCaseId2, "Test2");
		var tc3 = CreateTestCaseInfo(TestCaseId3, "Test3");
		var testCases = CreateTestCaseDictionary(tc1, tc2, tc3);

		var sink = new Xunit3ExecutionMessageSink(testCases, null);

		// tc1: passed
		sink.OnMessage(CreateTestPassed(TestCaseId1));
		sink.OnMessage(CreateTestFinished(TestCaseId1, 0.1m));
		sink.OnMessage(CreateTestCaseFinished(TestCaseId1));

		// tc2: failed
		sink.OnMessage(CreateTestFailed(TestCaseId2, "Fail"));
		sink.OnMessage(CreateTestFinished(TestCaseId2, 0.2m));
		sink.OnMessage(CreateTestCaseFinished(TestCaseId2));

		// tc3: skipped
		sink.OnMessage(CreateTestSkipped(TestCaseId3, "Skip"));
		sink.OnMessage(CreateTestFinished(TestCaseId3, 0.0m));
		sink.OnMessage(CreateTestCaseFinished(TestCaseId3));

		Assert.Equal(TestResultStatus.Passed, tc1.Result!.Status);
		Assert.Equal(TestResultStatus.Failed, tc2.Result!.Status);
		Assert.Equal(TestResultStatus.Skipped, tc3.Result!.Status);
	}

	#endregion
}
