using System.Globalization;
using System.Text.Json;

using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public class EventStreamFormatterTests
{
	[Fact]
	public void BeginTestRun_WritesBeginEvent()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();

		formatter.BeginTestRun(writer, "Starting tests");

		var output = writer.ToString().TrimEnd();
		var evt = JsonSerializer.Deserialize<TestResultEvent>(output);

		Assert.NotNull(evt);
		Assert.Equal(TestResultEvent.TypeBegin, evt.Type);
		Assert.Equal("Starting tests", evt.Message);
		Assert.NotNull(evt.Timestamp);
	}

	[Fact]
	public void BeginTestRun_WithNullMessage_OmitsMessage()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();

		formatter.BeginTestRun(writer);

		var output = writer.ToString().TrimEnd();
		var evt = JsonSerializer.Deserialize<TestResultEvent>(output);

		Assert.NotNull(evt);
		Assert.Equal(TestResultEvent.TypeBegin, evt.Type);
		Assert.Null(evt.Message);
	}

	[Fact]
	public void RecordResult_WritesResultEvent_ForPassedTest()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();
		formatter.BeginTestRun(writer);

		var resultInfo = CreateTestResult("Ns.Class.PassingTest", "test.dll", TestResultStatus.Passed, TimeSpan.FromMilliseconds(123));
		formatter.RecordResult(resultInfo);

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(2, lines.Length);

		var evt = JsonSerializer.Deserialize<TestResultEvent>(lines[1]);
		Assert.NotNull(evt);
		Assert.Equal(TestResultEvent.TypeResult, evt.Type);
		Assert.Equal("Ns.Class.PassingTest", evt.DisplayName);
		Assert.Equal("test.dll", evt.Assembly);
		Assert.Equal("Passed", evt.Status);
		Assert.Equal("00:00:00.1230000", evt.Duration);
		Assert.Null(evt.Output);
		Assert.Null(evt.ErrorMessage);
		Assert.Null(evt.ErrorStackTrace);
		Assert.Null(evt.SkipReason);
	}

	[Fact]
	public void RecordResult_WritesResultEvent_ForFailedTest()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();
		formatter.BeginTestRun(writer);

		var resultInfo = CreateTestResult(
			"Ns.Class.FailTest", "test.dll", TestResultStatus.Failed,
			TimeSpan.FromMilliseconds(50),
			output: "console output",
			errorMessage: "Assert.Equal failed",
			errorStackTrace: "at Ns.Class.FailTest()");

		formatter.RecordResult(resultInfo);

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var evt = JsonSerializer.Deserialize<TestResultEvent>(lines[1]);

		Assert.NotNull(evt);
		Assert.Equal("Failed", evt.Status);
		Assert.Equal("console output", evt.Output);
		Assert.Equal("Assert.Equal failed", evt.ErrorMessage);
		Assert.Equal("at Ns.Class.FailTest()", evt.ErrorStackTrace);
	}

	[Fact]
	public void RecordResult_WritesResultEvent_ForSkippedTest()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();
		formatter.BeginTestRun(writer);

		var resultInfo = CreateTestResult(
			"Ns.Class.SkipTest", "test.dll", TestResultStatus.Skipped,
			TimeSpan.Zero, skipReason: "Not implemented yet");

		formatter.RecordResult(resultInfo);

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var evt = JsonSerializer.Deserialize<TestResultEvent>(lines[1]);

		Assert.NotNull(evt);
		Assert.Equal("Skipped", evt.Status);
		Assert.Equal("Not implemented yet", evt.SkipReason);
	}

	[Fact]
	public void EndTestRun_WritesEndEvent()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();
		formatter.BeginTestRun(writer);
		formatter.EndTestRun();

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(2, lines.Length);

		var evt = JsonSerializer.Deserialize<TestResultEvent>(lines[1]);
		Assert.NotNull(evt);
		Assert.Equal(TestResultEvent.TypeEnd, evt.Type);
		Assert.NotNull(evt.Timestamp);
	}

	[Fact]
	public void FullRun_ProducesValidNdjson()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();

		formatter.BeginTestRun(writer, "Test run");
		formatter.RecordResult(CreateTestResult("Test1", "a.dll", TestResultStatus.Passed, TimeSpan.FromSeconds(1)));
		formatter.RecordResult(CreateTestResult("Test2", "a.dll", TestResultStatus.Failed, TimeSpan.FromSeconds(2), errorMessage: "boom"));
		formatter.EndTestRun();

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(4, lines.Length);

		// Each line must be valid JSON
		foreach (var line in lines)
		{
			var doc = JsonDocument.Parse(line);
			Assert.NotNull(doc);
		}
	}

	static ITestResultInfo CreateTestResult(
		string displayName, string assembly, TestResultStatus status, TimeSpan duration,
		string? output = null, string? errorMessage = null, string? errorStackTrace = null, string? skipReason = null)
	{
		var testAssembly = Substitute.For<ITestAssemblyInfo>();
		testAssembly.AssemblyFileName.Returns(assembly);

		var testCase = Substitute.For<ITestCaseInfo>();
		testCase.DisplayName.Returns(displayName);
		testCase.TestAssembly.Returns(testAssembly);

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
}

public class TestResultEventParseTests
{
	[Fact]
	public void Parse_NullOrEmpty_ReturnsNull()
	{
		Assert.Null(TestResultEvent.Parse(null!));
		Assert.Null(TestResultEvent.Parse(""));
		Assert.Null(TestResultEvent.Parse("   "));
	}

	[Fact]
	public void Parse_InvalidJson_ReturnsNull()
	{
		Assert.Null(TestResultEvent.Parse("not json at all"));
	}

	[Fact]
	public void Parse_ValidBeginEvent_ReturnsEvent()
	{
		var json = """{"type":"begin","message":"Starting","timestamp":"2026-01-01T00:00:00Z"}""";
		var evt = TestResultEvent.Parse(json);

		Assert.NotNull(evt);
		Assert.Equal("begin", evt.Type);
		Assert.Equal("Starting", evt.Message);
		Assert.Equal("2026-01-01T00:00:00Z", evt.Timestamp);
	}

	[Fact]
	public void Parse_ValidResultEvent_ReturnsEvent()
	{
		var json = """{"type":"result","displayName":"MyTest","assembly":"test.dll","status":"Passed","duration":"00:00:01.5000000"}""";
		var evt = TestResultEvent.Parse(json);

		Assert.NotNull(evt);
		Assert.Equal("result", evt.Type);
		Assert.Equal("MyTest", evt.DisplayName);
		Assert.Equal("test.dll", evt.Assembly);
		Assert.Equal("Passed", evt.Status);
		Assert.Equal("00:00:01.5000000", evt.Duration);
	}

	[Fact]
	public void Parse_ValidEndEvent_ReturnsEvent()
	{
		var json = """{"type":"end","timestamp":"2026-01-01T00:01:00Z"}""";
		var evt = TestResultEvent.Parse(json);

		Assert.NotNull(evt);
		Assert.Equal("end", evt.Type);
	}

	[Fact]
	public void ToInfo_PassedTest_ReconstructsCorrectly()
	{
		var evt = new TestResultEvent
		{
			Type = "result",
			DisplayName = "Ns.Class.TestMethod",
			Assembly = "MyAssembly.dll",
			Status = "Passed",
			Duration = "00:00:00.2500000",
		};

		var result = evt.ToInfo();

		Assert.Equal("Ns.Class.TestMethod", result.TestCase.DisplayName);
		Assert.Equal("MyAssembly.dll", result.TestCase.TestAssembly.AssemblyFileName);
		Assert.Equal(TestResultStatus.Passed, result.Status);
		Assert.Equal(TimeSpan.FromMilliseconds(250), result.Duration);
		Assert.Null(result.Output);
		Assert.Null(result.ErrorMessage);
		Assert.Null(result.ErrorStackTrace);
		Assert.Null(result.SkipReason);
	}

	[Fact]
	public void ToInfo_FailedTest_ReconstructsCorrectly()
	{
		var evt = new TestResultEvent
		{
			Type = "result",
			DisplayName = "Ns.Class.FailTest",
			Assembly = "test.dll",
			Status = "Failed",
			Duration = "00:00:00.0500000",
			Output = "console output",
			ErrorMessage = "Assert.Equal failed",
			ErrorStackTrace = "at Ns.Class.FailTest()",
		};

		var result = evt.ToInfo();

		Assert.Equal(TestResultStatus.Failed, result.Status);
		Assert.Equal("console output", result.Output);
		Assert.Equal("Assert.Equal failed", result.ErrorMessage);
		Assert.Equal("at Ns.Class.FailTest()", result.ErrorStackTrace);
	}

	[Fact]
	public void ToInfo_SkippedTest_ReconstructsCorrectly()
	{
		var evt = new TestResultEvent
		{
			Type = "result",
			DisplayName = "Ns.Class.SkipTest",
			Assembly = "test.dll",
			Status = "Skipped",
			Duration = "00:00:00",
			SkipReason = "Not implemented yet",
		};

		var result = evt.ToInfo();

		Assert.Equal(TestResultStatus.Skipped, result.Status);
		Assert.Equal("Not implemented yet", result.SkipReason);
	}

	[Fact]
	public void ToInfo_UnknownStatus_DefaultsToNotRun()
	{
		var evt = new TestResultEvent
		{
			Type = "result",
			DisplayName = "Test",
			Assembly = "test.dll",
			Status = "Unknown",
			Duration = "00:00:00",
		};

		var result = evt.ToInfo();
		Assert.Equal(TestResultStatus.NotRun, result.Status);
	}

	[Fact]
	public void RoundTrip_FormatterThenParse_PreservesData()
	{
		var writer = new StringWriter();
		var formatter = new EventStreamFormatter();

		var testAssembly = Substitute.For<ITestAssemblyInfo>();
		testAssembly.AssemblyFileName.Returns("roundtrip.dll");

		var testCase = Substitute.For<ITestCaseInfo>();
		testCase.DisplayName.Returns("Ns.RoundTrip.Test(x: 42)");
		testCase.TestAssembly.Returns(testAssembly);

		var originalResult = Substitute.For<ITestResultInfo>();
		originalResult.TestCase.Returns(testCase);
		originalResult.Status.Returns(TestResultStatus.Failed);
		originalResult.Duration.Returns(TimeSpan.FromMilliseconds(777));
		originalResult.Output.Returns("test output here");
		originalResult.ErrorMessage.Returns("Expected 42 but got 0");
		originalResult.ErrorStackTrace.Returns("at Ns.RoundTrip.Test()");
		originalResult.SkipReason.Returns((string?)null);

		formatter.BeginTestRun(writer, "Round trip test");
		formatter.RecordResult(originalResult);
		formatter.EndTestRun();

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(3, lines.Length);

		// Parse the result event and convert back
		var evt = TestResultEvent.Parse(lines[1]);
		Assert.NotNull(evt);

		var reconstructed = evt.ToInfo();

		Assert.Equal("Ns.RoundTrip.Test(x: 42)", reconstructed.TestCase.DisplayName);
		Assert.Equal("roundtrip.dll", reconstructed.TestCase.TestAssembly.AssemblyFileName);
		Assert.Equal(TestResultStatus.Failed, reconstructed.Status);
		Assert.Equal(TimeSpan.FromMilliseconds(777), reconstructed.Duration);
		Assert.Equal("test output here", reconstructed.Output);
		Assert.Equal("Expected 42 but got 0", reconstructed.ErrorMessage);
		Assert.Equal("at Ns.RoundTrip.Test()", reconstructed.ErrorStackTrace);
		Assert.Null(reconstructed.SkipReason);
	}

	[Fact]
	public void FromInfo_CreatesResultEvent()
	{
		var testAssembly = Substitute.For<ITestAssemblyInfo>();
		testAssembly.AssemblyFileName.Returns("my.dll");

		var testCase = Substitute.For<ITestCaseInfo>();
		testCase.DisplayName.Returns("Ns.MyTest");
		testCase.TestAssembly.Returns(testAssembly);

		var info = Substitute.For<ITestResultInfo>();
		info.TestCase.Returns(testCase);
		info.Status.Returns(TestResultStatus.Passed);
		info.Duration.Returns(TimeSpan.FromSeconds(1));
		info.Output.Returns("hello");

		var evt = TestResultEvent.FromInfo(info);

		Assert.Equal(TestResultEvent.TypeResult, evt.Type);
		Assert.Equal("Ns.MyTest", evt.DisplayName);
		Assert.Equal("my.dll", evt.Assembly);
		Assert.Equal("Passed", evt.Status);
		Assert.Equal("00:00:01", evt.Duration);
		Assert.Equal("hello", evt.Output);
		Assert.NotNull(evt.Timestamp);
	}

	[Fact]
	public void Begin_CreatesBeginEvent()
	{
		var evt = TestResultEvent.Begin("starting");

		Assert.Equal(TestResultEvent.TypeBegin, evt.Type);
		Assert.Equal("starting", evt.Message);
		Assert.NotNull(evt.Timestamp);
	}

	[Fact]
	public void End_CreatesEndEvent()
	{
		var evt = TestResultEvent.End();

		Assert.Equal(TestResultEvent.TypeEnd, evt.Type);
		Assert.NotNull(evt.Timestamp);
	}
}
