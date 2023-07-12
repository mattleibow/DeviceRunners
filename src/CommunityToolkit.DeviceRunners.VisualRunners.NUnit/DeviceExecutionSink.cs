// using NUnit;
// using NUnit.Abstractions;

// namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

// class DeviceExecutionSink : TestMessageSink
// {
// 	readonly IReadOnlyDictionary<ITestCase, NUnitTestCaseInfo> _testCases;

// 	public DeviceExecutionSink(IReadOnlyDictionary<ITestCase, NUnitTestCaseInfo> testCases)
// 	{
// 		_testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));

// 		Execution.TestFailedEvent += HandleTestFailed;
// 		Execution.TestPassedEvent += HandleTestPassed;
// 		Execution.TestSkippedEvent += HandleTestSkipped;
// 	}

// 	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args) =>
// 		RecordResult(args.Message, TestResultStatus.Failed);

// 	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args) =>
// 		RecordResult(args.Message, TestResultStatus.Passed);

// 	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args) =>
// 		RecordResult(args.Message, TestResultStatus.Skipped);

// 	void RecordResult(ITestResultMessage testResult, TestResultStatus status)
// 	{
// 		if (!_testCases.TryGetValue(testResult.TestCase, out NUnitTestCaseInfo? testCase))
// 		{
// 			// no matching reference, search by Unique ID as a fallback
// 			testCase = _testCases.FirstOrDefault(kvp => kvp.Key.UniqueID?.Equals(testResult.TestCase.UniqueID, StringComparison.Ordinal) ?? false).Value;

// 			// no tests found, so we don't know what to do
// 			if (testCase == null)
// 				return;
// 		}

// 		var result = new NUnitTestResultInfo(testCase, testResult, status);
// 		testCase.ReportResult(result);
// 	}
// }
