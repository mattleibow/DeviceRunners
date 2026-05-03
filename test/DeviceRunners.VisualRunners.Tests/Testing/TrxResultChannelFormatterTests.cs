using System.Xml.Linq;

using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public class TrxResultChannelFormatterTests
{
	const string TrxNs = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
	const string ExpectedTestListId = "8c84fa94-04c1-424b-9868-57a2d4851a1d";

	static XDocument RunFormatter(Action<TrxResultChannelFormatter> configure, IEnumerable<ITestResultInfo> results)
	{
		var formatter = new TrxResultChannelFormatter();
		configure(formatter);

		var sw = new StringWriter();
		formatter.BeginTestRun(sw);
		foreach (var result in results)
			formatter.RecordResult(result);
		formatter.EndTestRun();

		return XDocument.Parse(sw.ToString());
	}

	static ITestResultInfo MakeResult(
		string displayName,
		TestResultStatus status,
		TimeSpan duration = default,
		string? output = null,
		string? errorMessage = null,
		string? errorStackTrace = null,
		string? skipReason = null,
		string assemblyFileName = "test.dll")
	{
		var assembly = Substitute.For<ITestAssemblyInfo>();
		assembly.AssemblyFileName.Returns(assemblyFileName);

		var testCase = Substitute.For<ITestCaseInfo>();
		testCase.DisplayName.Returns(displayName);
		testCase.TestAssembly.Returns(assembly);

		var result = Substitute.For<ITestResultInfo>();
		result.TestCase.Returns(testCase);
		result.Status.Returns(status);
		result.Duration.Returns(duration);
		result.Output.Returns(output);
		result.ErrorMessage.Returns(errorMessage);
		result.ErrorStackTrace.Returns(errorStackTrace);
		result.SkipReason.Returns(skipReason);

		return result;
	}

	static XElement Root(XDocument doc) => doc.Root!;

	static XNamespace Ns => TrxNs;

	[Fact]
	public void BeginTestRun_CreatesTestRunRootElement()
	{
		var doc = RunFormatter(_ => { }, []);

		Assert.Equal("TestRun", doc.Root!.Name.LocalName);
		Assert.Equal(TrxNs, doc.Root.Name.NamespaceName);
	}

	[Fact]
	public void BeginTestRun_TestRun_HasIdAttribute()
	{
		var doc = RunFormatter(_ => { }, []);

		var id = Root(doc).Attribute("id")?.Value;
		Assert.NotNull(id);
		Assert.True(Guid.TryParse(id, out _), $"id '{id}' should be a valid GUID");
	}

	[Fact]
	public void BeginTestRun_SetsRunNameAndUser()
	{
		var doc = RunFormatter(f =>
		{
			f.TestRunName = "MyTestRun";
			f.TestRunUser = "testuser";
		}, []);

		Assert.Equal("MyTestRun", Root(doc).Attribute("name")?.Value);
		Assert.Equal("testuser", Root(doc).Attribute("runUser")?.Value);
	}

	[Fact]
	public void BeginTestRun_ContainsRequiredChildElements()
	{
		var doc = RunFormatter(_ => { }, []);
		var root = Root(doc);

		Assert.NotNull(root.Element(Ns + "Times"));
		Assert.NotNull(root.Element(Ns + "Results"));
		Assert.NotNull(root.Element(Ns + "TestDefinitions"));
		Assert.NotNull(root.Element(Ns + "TestEntries"));
		Assert.NotNull(root.Element(Ns + "TestLists"));
	}

	[Fact]
	public void BeginTestRun_Times_HasTimestampAttributes()
	{
		var doc = RunFormatter(_ => { }, []);
		var times = Root(doc).Element(Ns + "Times")!;

		Assert.NotNull(times.Attribute("creation")?.Value);
		Assert.NotNull(times.Attribute("start")?.Value);
	}

	[Fact]
	public void BeginTestRun_TestLists_ContainsDefaultList()
	{
		var doc = RunFormatter(_ => { }, []);
		var testList = Root(doc).Element(Ns + "TestLists")!.Element(Ns + "TestList")!;

		Assert.Equal(ExpectedTestListId, testList.Attribute("id")?.Value);
		Assert.Equal("Results Not in a List", testList.Attribute("name")?.Value);
	}

	[Fact]
	public void RecordResult_PassedTest_OutcomeIsPassed()
	{
		var result = MakeResult("MyTests.PassTest", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Equal("Passed", unitTestResult.Attribute("outcome")?.Value);
	}

	[Fact]
	public void RecordResult_FailedTest_OutcomeIsFailed()
	{
		var result = MakeResult("MyTests.FailTest", TestResultStatus.Failed, errorMessage: "Assert failed", errorStackTrace: "at MyTests.FailTest()");
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Equal("Failed", unitTestResult.Attribute("outcome")?.Value);
	}

	[Fact]
	public void RecordResult_SkippedTest_OutcomeIsNotExecuted()
	{
		var result = MakeResult("MyTests.SkipTest", TestResultStatus.Skipped);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Equal("NotExecuted", unitTestResult.Attribute("outcome")?.Value);
	}

	[Fact]
	public void RecordResult_SetsTestName()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Equal("MyTests.MyMethod", unitTestResult.Attribute("testName")?.Value);
	}

	[Fact]
	public void RecordResult_HasValidTestIdAndExecutionId()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var testId = unitTestResult.Attribute("testId")?.Value;
		var executionId = unitTestResult.Attribute("executionId")?.Value;

		Assert.True(Guid.TryParse(testId, out _), $"testId '{testId}' should be a valid GUID");
		Assert.True(Guid.TryParse(executionId, out _), $"executionId '{executionId}' should be a valid GUID");
	}

	[Fact]
	public void RecordResult_DurationIsInCorrectFormat()
	{
		var duration = TimeSpan.FromMilliseconds(1234);
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed, duration: duration);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var durationStr = unitTestResult.Attribute("duration")?.Value;

		Assert.NotNull(durationStr);
		Assert.True(TimeSpan.TryParse(durationStr, out var parsed), $"duration '{durationStr}' should be parseable as TimeSpan");
		Assert.Equal(duration, parsed);
	}

	[Fact]
	public void RecordResult_HasTestListId()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Equal(ExpectedTestListId, unitTestResult.Attribute("testListId")?.Value);
	}

	[Fact]
	public void RecordResult_FailedTest_HasOutputWithErrorInfo()
	{
		var result = MakeResult("MyTests.FailTest", TestResultStatus.Failed,
			errorMessage: "Expected true but got false",
			errorStackTrace: "at MyTests.FailTest() in MyTests.cs:line 10");
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var output = unitTestResult.Element(Ns + "Output")!;
		var errorInfo = output.Element(Ns + "ErrorInfo")!;

		Assert.Equal("Expected true but got false", errorInfo.Element(Ns + "Message")!.Value);
		Assert.Equal("at MyTests.FailTest() in MyTests.cs:line 10", errorInfo.Element(Ns + "StackTrace")!.Value);
	}

	[Fact]
	public void RecordResult_PassedTest_WithNoOutput_HasNoOutputElement()
	{
		var result = MakeResult("MyTests.PassTest", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Null(unitTestResult.Element(Ns + "Output"));
	}

	[Fact]
	public void RecordResult_PassedTest_WithOutput_HasStdOutElement()
	{
		var result = MakeResult("MyTests.PassTest", TestResultStatus.Passed, output: "some output");
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		Assert.Equal("some output", unitTestResult.Element(Ns + "Output")!.Element(Ns + "StdOut")!.Value);
	}

	[Fact]
	public void RecordResult_AddsUnitTestToTestDefinitions()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed, assemblyFileName: "mytest.dll");
		var doc = RunFormatter(_ => { }, [result]);

		var unitTest = Root(doc).Element(Ns + "TestDefinitions")!.Element(Ns + "UnitTest")!;
		Assert.NotNull(unitTest);
		Assert.Equal("MyTests.MyMethod", unitTest.Attribute("name")?.Value);
		Assert.Equal("mytest.dll", unitTest.Attribute("storage")?.Value);
	}

	[Fact]
	public void RecordResult_UnitTest_HasExecutionElement()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTest = Root(doc).Element(Ns + "TestDefinitions")!.Element(Ns + "UnitTest")!;
		var execution = unitTest.Element(Ns + "Execution")!;
		Assert.NotNull(execution);
		Assert.True(Guid.TryParse(execution.Attribute("id")?.Value, out _));
	}

	[Fact]
	public void RecordResult_UnitTest_HasTestMethodElement()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed, assemblyFileName: "mytest.dll");
		var doc = RunFormatter(_ => { }, [result]);

		var unitTest = Root(doc).Element(Ns + "TestDefinitions")!.Element(Ns + "UnitTest")!;
		var testMethod = unitTest.Element(Ns + "TestMethod")!;
		Assert.NotNull(testMethod);
		// name is just the method segment; className is the prefix
		Assert.Equal("MyMethod", testMethod.Attribute("name")?.Value);
		Assert.Equal("MyTests", testMethod.Attribute("className")?.Value);
		Assert.Equal("mytest.dll", testMethod.Attribute("codeBase")?.Value);
	}

	[Fact]
	public void RecordResult_AddsTestEntry()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testEntry = Root(doc).Element(Ns + "TestEntries")!.Element(Ns + "TestEntry")!;
		Assert.NotNull(testEntry);
		Assert.Equal(ExpectedTestListId, testEntry.Attribute("testListId")?.Value);
		Assert.True(Guid.TryParse(testEntry.Attribute("testId")?.Value, out _));
		Assert.True(Guid.TryParse(testEntry.Attribute("executionId")?.Value, out _));
	}

	[Fact]
	public void RecordResult_IdsAreConsistentAcrossResult_Definition_Entry()
	{
		var result = MakeResult("MyTests.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var unitTest = Root(doc).Element(Ns + "TestDefinitions")!.Element(Ns + "UnitTest")!;
		var testEntry = Root(doc).Element(Ns + "TestEntries")!.Element(Ns + "TestEntry")!;

		var testId = unitTestResult.Attribute("testId")!.Value;
		var executionId = unitTestResult.Attribute("executionId")!.Value;

		// testId cross-links UnitTestResult <-> UnitTest <-> TestEntry
		Assert.Equal(testId, unitTest.Attribute("id")!.Value);
		Assert.Equal(testId, testEntry.Attribute("testId")!.Value);

		// executionId cross-links UnitTestResult <-> Execution <-> TestEntry
		Assert.Equal(executionId, unitTest.Element(Ns + "Execution")!.Attribute("id")!.Value);
		Assert.Equal(executionId, testEntry.Attribute("executionId")!.Value);
	}

	[Fact]
	public void EndTestRun_AddsResultSummaryWithOutcomePassed_WhenNoFailures()
	{
		var doc = RunFormatter(_ => { }, []);

		var resultSummary = Root(doc).Element(Ns + "ResultSummary")!;
		Assert.NotNull(resultSummary);
		Assert.Equal("Passed", resultSummary.Attribute("outcome")?.Value);
	}

	[Fact]
	public void EndTestRun_Counters_ArePresent()
	{
		var doc = RunFormatter(_ => { }, []);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.NotNull(counters);
	}

	[Fact]
	public void EndTestRun_Counters_ReflectPassedTests()
	{
		var results = new[]
		{
			MakeResult("T.Pass1", TestResultStatus.Passed),
			MakeResult("T.Pass2", TestResultStatus.Passed),
		};
		var doc = RunFormatter(_ => { }, results);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("2", counters.Attribute("passed")?.Value);
		Assert.Equal("0", counters.Attribute("failed")?.Value);
		Assert.Equal("2", counters.Attribute("total")?.Value);
	}

	[Fact]
	public void EndTestRun_Counters_ReflectFailedTests()
	{
		var results = new[]
		{
			MakeResult("T.Pass1", TestResultStatus.Passed),
			MakeResult("T.Fail1", TestResultStatus.Failed, errorMessage: "oops"),
		};
		var doc = RunFormatter(_ => { }, results);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("1", counters.Attribute("passed")?.Value);
		Assert.Equal("1", counters.Attribute("failed")?.Value);
		Assert.Equal("2", counters.Attribute("total")?.Value);
	}

	[Fact]
	public void EndTestRun_Counters_SkippedTestsDoNotCountAsPassed()
	{
		var results = new[]
		{
			MakeResult("T.Pass1", TestResultStatus.Passed),
			MakeResult("T.Skip1", TestResultStatus.Skipped),
		};
		var doc = RunFormatter(_ => { }, results);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		// Skipped test must NOT be counted as passed
		Assert.Equal("1", counters.Attribute("passed")?.Value);
		Assert.Equal("0", counters.Attribute("failed")?.Value);
		Assert.Equal("2", counters.Attribute("total")?.Value);
	}

	[Fact]
	public void EndTestRun_Times_FinishIsUpdated()
	{
		var doc = RunFormatter(_ => { }, []);

		var times = Root(doc).Element(Ns + "Times")!;
		Assert.NotNull(times.Attribute("finish")?.Value);
	}

	[Fact]
	public void FullRun_MixedResults_ProducesCorrectXml()
	{
		var results = new[]
		{
			MakeResult("Suite.Pass", TestResultStatus.Passed, TimeSpan.FromMilliseconds(100)),
			MakeResult("Suite.Fail", TestResultStatus.Failed, TimeSpan.FromMilliseconds(50),
				errorMessage: "Assert.Equal() failure", errorStackTrace: "at Suite.Fail()"),
			MakeResult("Suite.Skip", TestResultStatus.Skipped),
		};
		var doc = RunFormatter(f => { f.TestRunName = "FullRun"; }, results);

		var resultsEl = Root(doc).Element(Ns + "Results")!;
		var unitTestResults = resultsEl.Elements(Ns + "UnitTestResult").ToList();
		Assert.Equal(3, unitTestResults.Count);

		var passed = unitTestResults.Single(e => e.Attribute("testName")?.Value == "Suite.Pass");
		var failed = unitTestResults.Single(e => e.Attribute("testName")?.Value == "Suite.Fail");
		var skipped = unitTestResults.Single(e => e.Attribute("testName")?.Value == "Suite.Skip");

		Assert.Equal("Passed", passed.Attribute("outcome")?.Value);
		Assert.Equal("Failed", failed.Attribute("outcome")?.Value);
		Assert.Equal("NotExecuted", skipped.Attribute("outcome")?.Value);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("1", counters.Attribute("passed")?.Value);
		Assert.Equal("1", counters.Attribute("failed")?.Value);
		Assert.Equal("3", counters.Attribute("total")?.Value);
	}

	// -------------------------------------------------------------------------
	// Golden schema validation — derived from real `dotnet test --logger trx` output
	// -------------------------------------------------------------------------

	// Golden: UnitTestResult.testName uses the full DisplayName
	[Fact]
	public void GoldenSchema_TestName_IsFullDisplayName()
	{
		var result = MakeResult("MyNamespace.MyClass.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testName = Root(doc).Element(Ns + "Results")!
			.Element(Ns + "UnitTestResult")!
			.Attribute("testName")!.Value;

		Assert.Equal("MyNamespace.MyClass.MyMethod", testName);
	}

	// Golden: TestMethod.name is just the method segment (last part of DisplayName)
	[Fact]
	public void GoldenSchema_TestMethod_Name_IsMethodNameOnly()
	{
		var result = MakeResult("MyNamespace.MyClass.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!;

		Assert.Equal("MyMethod", testMethod.Attribute("name")!.Value);
	}

	// Golden: TestMethod.className is the namespace + class (everything before the last dot)
	[Fact]
	public void GoldenSchema_TestMethod_ClassName_IsNamespaceAndClass()
	{
		var result = MakeResult("MyNamespace.MyClass.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!;

		Assert.Equal("MyNamespace.MyClass", testMethod.Attribute("className")!.Value);
	}

	// Golden: ResultSummary.outcome is "Failed" when any test fails
	[Fact]
	public void GoldenSchema_ResultSummary_Outcome_IsFailedWhenTestsFail()
	{
		var results = new[]
		{
			MakeResult("T.Pass", TestResultStatus.Passed),
			MakeResult("T.Fail", TestResultStatus.Failed, errorMessage: "oops"),
		};
		var doc = RunFormatter(_ => { }, results);

		Assert.Equal("Failed", Root(doc).Element(Ns + "ResultSummary")!.Attribute("outcome")!.Value);
	}

	// Golden: ResultSummary.outcome is "Passed" when all tests pass
	[Fact]
	public void GoldenSchema_ResultSummary_Outcome_IsPassedWhenAllPass()
	{
		var results = new[] { MakeResult("T.Pass", TestResultStatus.Passed) };
		var doc = RunFormatter(_ => { }, results);

		Assert.Equal("Passed", Root(doc).Element(Ns + "ResultSummary")!.Attribute("outcome")!.Value);
	}

	// Golden: Counters has "executed" attribute (passed + failed, skipped not counted)
	[Fact]
	public void GoldenSchema_Counters_HasExecutedAttribute()
	{
		var results = new[]
		{
			MakeResult("T.Pass", TestResultStatus.Passed),
			MakeResult("T.Fail", TestResultStatus.Failed, errorMessage: "oops"),
			MakeResult("T.Skip", TestResultStatus.Skipped),
		};
		var doc = RunFormatter(_ => { }, results);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("2", counters.Attribute("executed")!.Value);
	}

	// Golden: Counters has full set of attributes matching the TRX schema
	[Theory]
	[InlineData("total")]
	[InlineData("executed")]
	[InlineData("passed")]
	[InlineData("failed")]
	[InlineData("error")]
	[InlineData("timeout")]
	[InlineData("aborted")]
	[InlineData("inconclusive")]
	[InlineData("passedButRunAborted")]
	[InlineData("notRunnable")]
	[InlineData("notExecuted")]
	[InlineData("disconnected")]
	[InlineData("warning")]
	[InlineData("completed")]
	[InlineData("inProgress")]
	[InlineData("pending")]
	public void GoldenSchema_Counters_HasExpectedAttribute(string attrName)
	{
		var doc = RunFormatter(_ => { }, []);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.NotNull(counters.Attribute(attrName));
	}

	// Golden: skipped test with a reason emits Output/StdOut AND Output/ErrorInfo/Message
	// (NUnit golden confirms both nodes appear for ignored tests)
	[Fact]
	public void GoldenSchema_SkippedTest_WithReason_EmitsStdOutAndErrorInfoMessage()
	{
		var result = MakeResult("T.Skip", TestResultStatus.Skipped, skipReason: "Not ready yet");
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var output = unitTestResult.Element(Ns + "Output")!;

		Assert.Equal("Not ready yet", output.Element(Ns + "StdOut")!.Value);
		Assert.Equal("Not ready yet", output.Element(Ns + "ErrorInfo")!.Element(Ns + "Message")!.Value);
	}

	// Golden: display name without a dot falls back gracefully (no crash, testName = full name)
	[Fact]
	public void GoldenSchema_NoDotInDisplayName_FallsBackToFullNameForBoth()
	{
		var result = MakeResult("SimpleMethodName", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!;

		// When there's no dot, both name and className fall back to the full display name
		Assert.Equal("SimpleMethodName", testMethod.Attribute("name")!.Value);
		Assert.Equal("SimpleMethodName", testMethod.Attribute("className")!.Value);
	}

	// -------------------------------------------------------------------------
	// Theory / parameterized tests (xUnit [Theory], NUnit [TestCase])
	// -------------------------------------------------------------------------

	// xUnit theory: "NS.Class.Method(a: 1, b: 2)"
	// → TestMethod.name = "Method" (no params), className = "NS.Class"
	// → UnitTestResult.testName = full display name with params (for traceability)
	[Fact]
	public void Theory_TestName_IsFullDisplayNameIncludingParams()
	{
		var result = MakeResult("MyNS.MyClass.MyTheory(a: 1, b: 2)", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testName = Root(doc).Element(Ns + "Results")!
			.Element(Ns + "UnitTestResult")!
			.Attribute("testName")!.Value;

		Assert.Equal("MyNS.MyClass.MyTheory(a: 1, b: 2)", testName);
	}

	[Fact]
	public void Theory_TestMethod_Name_IsMethodNameWithoutParams()
	{
		var result = MakeResult("MyNS.MyClass.MyTheory(a: 1, b: 2)", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var name = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!
			.Attribute("name")!.Value;

		Assert.Equal("MyTheory", name);
	}

	[Fact]
	public void Theory_TestMethod_ClassName_IsCorrect()
	{
		var result = MakeResult("MyNS.MyClass.MyTheory(a: 1, b: 2)", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var className = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!
			.Attribute("className")!.Value;

		Assert.Equal("MyNS.MyClass", className);
	}

	// Dotted string parameter: "NS.Class.Method(value: \"hello.world\")"
	// The dot inside the param must NOT corrupt className/name.
	[Theory]
	[InlineData(@"MyNS.MyClass.Method(value: ""hello.world"")", "Method", "MyNS.MyClass")]
	[InlineData(@"MyNS.MyClass.Method(x: 1.5, y: 2.0)", "Method", "MyNS.MyClass")]
	[InlineData(@"MyNS.MyClass.Method(s: ""a.b.c.d"")", "Method", "MyNS.MyClass")]
	public void Theory_DotsInParams_DoNotCorruptClassOrMethodName(
		string displayName, string expectedMethod, string expectedClass)
	{
		var result = MakeResult(displayName, TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!;

		Assert.Equal(expectedMethod, testMethod.Attribute("name")!.Value);
		Assert.Equal(expectedClass, testMethod.Attribute("className")!.Value);
	}

	// NUnit parameterized: FullName = "NS.Class.Method(1,2)" (no spaces, no named params)
	[Fact]
	public void NUnit_TestCase_ParamStyle_NameAndClassAreCorrect()
	{
		var result = MakeResult("MyNS.MyClass.MyTestCase(1,2)", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!;

		Assert.Equal("MyTestCase", testMethod.Attribute("name")!.Value);
		Assert.Equal("MyNS.MyClass", testMethod.Attribute("className")!.Value);
	}

	// NUnit nested class: "NS.OuterClass+InnerClass.Method"
	[Fact]
	public void NUnit_NestedClass_NameAndClassAreCorrect()
	{
		var result = MakeResult("MyNS.OuterClass+InnerClass.MyMethod", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Element(Ns + "UnitTest")!
			.Element(Ns + "TestMethod")!;

		Assert.Equal("MyMethod", testMethod.Attribute("name")!.Value);
		Assert.Equal("MyNS.OuterClass+InnerClass", testMethod.Attribute("className")!.Value);
	}

	// Multiple theories from the same method — each gets its own UnitTest entry
	[Fact]
	public void Theory_MultipleVariants_EachHasOwnTestDefinitionEntry()
	{
		var results = new[]
		{
			MakeResult("NS.C.Theory(a: 1)", TestResultStatus.Passed),
			MakeResult("NS.C.Theory(a: 2)", TestResultStatus.Passed),
			MakeResult("NS.C.Theory(a: 3)", TestResultStatus.Failed, errorMessage: "oops"),
		};
		var doc = RunFormatter(_ => { }, results);

		var unitTests = Root(doc).Element(Ns + "TestDefinitions")!
			.Elements(Ns + "UnitTest").ToList();

		Assert.Equal(3, unitTests.Count);

		// All share the same TestMethod.name
		Assert.All(unitTests, ut =>
			Assert.Equal("Theory", ut.Element(Ns + "TestMethod")!.Attribute("name")!.Value));

		// UnitTest.name preserves the full display name with params for each variant
		var names = unitTests.Select(ut => ut.Attribute("name")!.Value).ToList();
		Assert.Contains("NS.C.Theory(a: 1)", names);
		Assert.Contains("NS.C.Theory(a: 2)", names);
		Assert.Contains("NS.C.Theory(a: 3)", names);
	}

	// -------------------------------------------------------------------------
	// Multiple assemblies
	// -------------------------------------------------------------------------

	[Fact]
	public void MultiAssembly_EachResultCarriesItsOwnStorage()
	{
		var results = new[]
		{
			MakeResult("A.Tests.PassA", TestResultStatus.Passed, assemblyFileName: "assembly-a.dll"),
			MakeResult("B.Tests.PassB", TestResultStatus.Passed, assemblyFileName: "assembly-b.dll"),
		};
		var doc = RunFormatter(_ => { }, results);

		var unitTests = Root(doc).Element(Ns + "TestDefinitions")!
			.Elements(Ns + "UnitTest").ToList();

		var storages = unitTests.Select(ut => ut.Attribute("storage")!.Value).ToList();
		Assert.Contains("assembly-a.dll", storages);
		Assert.Contains("assembly-b.dll", storages);
	}

	[Fact]
	public void MultiAssembly_TestMethod_CodeBaseMatchesAssembly()
	{
		var results = new[]
		{
			MakeResult("A.Tests.Pass", TestResultStatus.Passed, assemblyFileName: "assembly-a.dll"),
			MakeResult("B.Tests.Pass", TestResultStatus.Passed, assemblyFileName: "assembly-b.dll"),
		};
		var doc = RunFormatter(_ => { }, results);

		var testMethods = Root(doc).Element(Ns + "TestDefinitions")!
			.Elements(Ns + "UnitTest")
			.Select(ut => ut.Element(Ns + "TestMethod")!)
			.ToList();

		var codeBases = testMethods.Select(tm => tm.Attribute("codeBase")!.Value).ToList();
		Assert.Contains("assembly-a.dll", codeBases);
		Assert.Contains("assembly-b.dll", codeBases);
	}

	[Fact]
	public void MultiAssembly_AllResultsAppearInSingleTestRun()
	{
		var results = new[]
		{
			MakeResult("A.Tests.Pass", TestResultStatus.Passed, assemblyFileName: "a.dll"),
			MakeResult("B.Tests.Pass", TestResultStatus.Passed, assemblyFileName: "b.dll"),
			MakeResult("C.Tests.Fail", TestResultStatus.Failed, errorMessage: "boom", assemblyFileName: "c.dll"),
		};
		var doc = RunFormatter(_ => { }, results);

		Assert.Equal(3, Root(doc).Element(Ns + "Results")!.Elements(Ns + "UnitTestResult").Count());
		Assert.Equal(3, Root(doc).Element(Ns + "TestDefinitions")!.Elements(Ns + "UnitTest").Count());
		Assert.Equal(3, Root(doc).Element(Ns + "TestEntries")!.Elements(Ns + "TestEntry").Count());

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("3", counters.Attribute("total")!.Value);
		Assert.Equal("1", counters.Attribute("failed")!.Value);
	}

	// -------------------------------------------------------------------------
	// Golden spec comparison — each case is drawn directly from a real golden TRX
	// generated by `dotnet test --logger trx`. These are the authoritative names.
	//
	// xUnit golden: test\DeviceRunners.VisualRunners.Tests\TestData\golden-xunit.trx
	// NUnit golden: test\DeviceRunners.VisualRunners.Tests\TestData\golden-nunit.trx
	// -------------------------------------------------------------------------

	// For each golden test case: given the display name (as ITestCaseInfo.DisplayName provides
	// it — full qualified name), verify our formatter produces the correct TestMethod.name,
	// TestMethod.className, and UnitTestResult.outcome.
	[Theory]
	// xUnit Facts (FullName = "Namespace.Class.Method")
	[InlineData("DeviceRunners.TrxGolden.GoldenTests.PassTest", "Passed", "PassTest", "DeviceRunners.TrxGolden.GoldenTests")]
	[InlineData("DeviceRunners.TrxGolden.GoldenTests.FailTest", "Failed", "FailTest", "DeviceRunners.TrxGolden.GoldenTests")]
	[InlineData("DeviceRunners.TrxGolden.GoldenTests.SkipTest", "NotExecuted", "SkipTest", "DeviceRunners.TrxGolden.GoldenTests")]
	// xUnit Theories — TestMethod.name strips params, matching xUnit TRX adapter behaviour
	[InlineData("DeviceRunners.TrxGolden.GoldenTests.Theory_Int(a: 2, b: 2)", "Passed", "Theory_Int", "DeviceRunners.TrxGolden.GoldenTests")]
	[InlineData("DeviceRunners.TrxGolden.GoldenTests.Theory_Int(a: 3, b: 3)", "Passed", "Theory_Int", "DeviceRunners.TrxGolden.GoldenTests")]
	[InlineData(@"DeviceRunners.TrxGolden.GoldenTests.Theory_StringWithDots(value: ""hello.world"")", "Passed", "Theory_StringWithDots", "DeviceRunners.TrxGolden.GoldenTests")]
	[InlineData(@"DeviceRunners.TrxGolden.GoldenTests.Theory_StringWithDots(value: ""foo.bar.baz"")", "Passed", "Theory_StringWithDots", "DeviceRunners.TrxGolden.GoldenTests")]
	// NUnit — ITest.FullName provides the full qualified name; params use positional syntax "(1,1)"
	[InlineData("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.PassTest", "Passed", "PassTest", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	[InlineData("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.FailTest", "Failed", "FailTest", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	[InlineData("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.IgnoreTest", "NotExecuted", "IgnoreTest", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	[InlineData("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_Int(1,1)", "Passed", "TestCase_Int", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	[InlineData("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_Int(2,3)", "Failed", "TestCase_Int", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	[InlineData(@"DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_StringWithDots(""hello.world"")", "Passed", "TestCase_StringWithDots", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	[InlineData(@"DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_StringWithDots(""foo.bar.baz"")", "Passed", "TestCase_StringWithDots", "DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests")]
	public void GoldenSpec_TestMethod_NameClassAndOutcome_AreCorrect(
		string displayName, string expectedOutcome,
		string expectedMethodName, string expectedClassName)
	{
		var status = expectedOutcome switch
		{
			"Passed" => TestResultStatus.Passed,
			"Failed" => TestResultStatus.Failed,
			_ => TestResultStatus.Skipped,
		};
		var result = MakeResult(displayName, status);
		var doc = RunFormatter(_ => { }, [result]);

		// UnitTestResult.testName = full display name (with params if any)
		var unitTestResult = Root(doc).Element(Ns + "Results")!
			.Elements(Ns + "UnitTestResult")
			.Single(r => r.Attribute("testName")!.Value == displayName);
		Assert.Equal(expectedOutcome, unitTestResult.Attribute("outcome")!.Value);

		// TestMethod.name = method only (no params); TestMethod.className = full class
		var testMethod = Root(doc).Element(Ns + "TestDefinitions")!
			.Elements(Ns + "UnitTest")
			.Single(ut => ut.Attribute("name")!.Value == displayName)
			.Element(Ns + "TestMethod")!;
		Assert.Equal(expectedMethodName, testMethod.Attribute("name")!.Value);
		Assert.Equal(expectedClassName, testMethod.Attribute("className")!.Value);
	}

	// Golden: combined xUnit + NUnit results in one TRX (multi-assembly scenario)
	[Fact]
	public void GoldenSpec_MultiAssembly_XUnitAndNUnit_CombinedInOneRun()
	{
		// Replicate the full golden fixture: 7 xUnit + 7 NUnit = 14 tests
		var xunitAssembly = "DeviceRunners.TrxGolden.dll";
		var nunitAssembly = "DeviceRunners.TrxGolden.NUnit.dll";

		var results = new[]
		{
			MakeResult("DeviceRunners.TrxGolden.GoldenTests.PassTest", TestResultStatus.Passed, assemblyFileName: xunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.GoldenTests.FailTest", TestResultStatus.Failed, errorMessage: "Assert.Equal() Failure", assemblyFileName: xunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.GoldenTests.SkipTest", TestResultStatus.Skipped, skipReason: "Intentionally skipped", assemblyFileName: xunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.GoldenTests.Theory_Int(a: 2, b: 2)", TestResultStatus.Passed, assemblyFileName: xunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.GoldenTests.Theory_Int(a: 3, b: 3)", TestResultStatus.Passed, assemblyFileName: xunitAssembly),
			MakeResult(@"DeviceRunners.TrxGolden.GoldenTests.Theory_StringWithDots(value: ""hello.world"")", TestResultStatus.Passed, assemblyFileName: xunitAssembly),
			MakeResult(@"DeviceRunners.TrxGolden.GoldenTests.Theory_StringWithDots(value: ""foo.bar.baz"")", TestResultStatus.Passed, assemblyFileName: xunitAssembly),

			MakeResult("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.PassTest", TestResultStatus.Passed, assemblyFileName: nunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.FailTest", TestResultStatus.Failed, errorMessage: "Assert failed", assemblyFileName: nunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.IgnoreTest", TestResultStatus.Skipped, skipReason: "Intentionally ignored", assemblyFileName: nunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_Int(1,1)", TestResultStatus.Passed, assemblyFileName: nunitAssembly),
			MakeResult("DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_Int(2,3)", TestResultStatus.Failed, errorMessage: "Assert failed", assemblyFileName: nunitAssembly),
			MakeResult(@"DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_StringWithDots(""hello.world"")", TestResultStatus.Passed, assemblyFileName: nunitAssembly),
			MakeResult(@"DeviceRunners.TrxGolden.NUnit.GoldenNUnitTests.TestCase_StringWithDots(""foo.bar.baz"")", TestResultStatus.Passed, assemblyFileName: nunitAssembly),
		};

		var doc = RunFormatter(_ => { }, results);

		// 14 results, definitions, and entries
		Assert.Equal(14, Root(doc).Element(Ns + "Results")!.Elements(Ns + "UnitTestResult").Count());
		Assert.Equal(14, Root(doc).Element(Ns + "TestDefinitions")!.Elements(Ns + "UnitTest").Count());
		Assert.Equal(14, Root(doc).Element(Ns + "TestEntries")!.Elements(Ns + "TestEntry").Count());

		// Counters: 9 pass, 3 fail, 2 skip → total=14, executed=12, passed=9, failed=3, notExecuted=0
		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("14", counters.Attribute("total")!.Value);
		Assert.Equal("12", counters.Attribute("executed")!.Value);
		Assert.Equal("9", counters.Attribute("passed")!.Value);
		Assert.Equal("3", counters.Attribute("failed")!.Value);
		Assert.Equal("0", counters.Attribute("notExecuted")!.Value);

		// ResultSummary.outcome = Failed (because there are failures)
		Assert.Equal("Failed", Root(doc).Element(Ns + "ResultSummary")!.Attribute("outcome")!.Value);

		// Each assembly's results carry the correct storage
		var xunitStorages = Root(doc).Element(Ns + "TestDefinitions")!
			.Elements(Ns + "UnitTest")
			.Where(ut => ut.Attribute("storage")!.Value == xunitAssembly)
			.ToList();
		var nunitStorages = Root(doc).Element(Ns + "TestDefinitions")!
			.Elements(Ns + "UnitTest")
			.Where(ut => ut.Attribute("storage")!.Value == nunitAssembly)
			.ToList();
		Assert.Equal(7, xunitStorages.Count);
		Assert.Equal(7, nunitStorages.Count);
	}

	// Golden: Times element has all four required attributes
	[Theory]
	[InlineData("creation")]
	[InlineData("queuing")]
	[InlineData("start")]
	[InlineData("finish")]
	public void GoldenSchema_Times_HasAttribute(string attrName)
	{
		var doc = RunFormatter(_ => { }, []);
		var times = Root(doc).Element(Ns + "Times")!;
		Assert.NotNull(times.Attribute(attrName));
	}

	// Golden: UnitTestResult has startTime and endTime attributes
	[Fact]
	public void GoldenSchema_UnitTestResult_HasStartTimeAndEndTime()
	{
		var result = MakeResult("T.Pass", TestResultStatus.Passed, duration: TimeSpan.FromMilliseconds(123));
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var startTime = unitTestResult.Attribute("startTime")?.Value;
		var endTime = unitTestResult.Attribute("endTime")?.Value;

		Assert.NotNull(startTime);
		Assert.NotNull(endTime);
		Assert.True(DateTimeOffset.TryParse(startTime, out _), $"startTime '{startTime}' must be a valid DateTimeOffset");
		Assert.True(DateTimeOffset.TryParse(endTime, out _), $"endTime '{endTime}' must be a valid DateTimeOffset");
	}

	// Golden: startTime is earlier than or equal to endTime, and their difference equals duration
	[Fact]
	public void GoldenSchema_UnitTestResult_StartAndEndTime_DifferenceEqualsDuration()
	{
		var duration = TimeSpan.FromSeconds(1.5);
		var result = MakeResult("T.Pass", TestResultStatus.Passed, duration: duration);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var startTime = DateTimeOffset.Parse(unitTestResult.Attribute("startTime")!.Value);
		var endTime = DateTimeOffset.Parse(unitTestResult.Attribute("endTime")!.Value);

		Assert.True(endTime >= startTime, "endTime must not be earlier than startTime");
		Assert.Equal(duration, endTime - startTime);
	}

	// Golden: UnitTestResult has relativeResultsDirectory = executionId
	[Fact]
	public void GoldenSchema_UnitTestResult_RelativeResultsDirectory_EqualsExecutionId()
	{
		var result = MakeResult("T.Pass", TestResultStatus.Passed);
		var doc = RunFormatter(_ => { }, [result]);

		var unitTestResult = Root(doc).Element(Ns + "Results")!.Element(Ns + "UnitTestResult")!;
		var executionId = unitTestResult.Attribute("executionId")!.Value;
		var relDir = unitTestResult.Attribute("relativeResultsDirectory")?.Value;

		Assert.Equal(executionId, relDir);
	}

	// Golden: TestLists contains both "Results Not in a List" and "All Loaded Results"
	[Theory]
	[InlineData("Results Not in a List", "8c84fa94-04c1-424b-9868-57a2d4851a1d")]
	[InlineData("All Loaded Results", "19431567-8539-422a-85d7-44ee4e166bda")]
	public void GoldenSchema_TestLists_ContainsBothRequiredLists(string name, string expectedId)
	{
		var doc = RunFormatter(_ => { }, []);

		var testList = Root(doc).Element(Ns + "TestLists")!
			.Elements(Ns + "TestList")
			.Single(tl => tl.Attribute("name")!.Value == name);

		Assert.Equal(expectedId, testList.Attribute("id")!.Value);
	}

	// Golden: notExecuted counter is always 0 (confirmed by both xUnit and NUnit goldens:
	// explicitly skipped/ignored tests are not counted in notExecuted)
	[Fact]
	public void GoldenSchema_Counters_NotExecuted_IsAlwaysZero()
	{
		var results = new[]
		{
			MakeResult("T.Pass", TestResultStatus.Passed),
			MakeResult("T.Fail", TestResultStatus.Failed, errorMessage: "x"),
			MakeResult("T.Skip", TestResultStatus.Skipped, skipReason: "reason"),
		};
		var doc = RunFormatter(_ => { }, results);

		var counters = Root(doc).Element(Ns + "ResultSummary")!.Element(Ns + "Counters")!;
		Assert.Equal("0", counters.Attribute("notExecuted")!.Value);
	}

	// Golden: failed test with per-test output gets StdOut before ErrorInfo
	[Fact]
	public void GoldenSchema_FailedTest_WithOutput_HasStdOutBeforeErrorInfo()
	{
		var result = MakeResult("T.Fail", TestResultStatus.Failed,
			output: "captured stdout", errorMessage: "boom");
		var doc = RunFormatter(_ => { }, [result]);

		var output = Root(doc).Element(Ns + "Results")!
			.Element(Ns + "UnitTestResult")!
			.Element(Ns + "Output")!;

		Assert.Equal("captured stdout", output.Element(Ns + "StdOut")!.Value);
		Assert.Equal("boom", output.Element(Ns + "ErrorInfo")!.Element(Ns + "Message")!.Value);
		// StdOut must come before ErrorInfo (matching the TRX schema order)
		var children = output.Elements().Select(e => e.Name.LocalName).ToList();
		Assert.Equal(new[] { "StdOut", "ErrorInfo" }, children);
	}
}
