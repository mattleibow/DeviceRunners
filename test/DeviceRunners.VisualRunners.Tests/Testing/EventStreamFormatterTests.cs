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
		var evt = TestResultEvent.Parse(output);

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
		var evt = TestResultEvent.Parse(output);

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

		var resultInfo = TestHelpers.CreateTestResult("Ns.Class.PassingTest", "test.dll", TestResultStatus.Passed, TimeSpan.FromMilliseconds(123));
		formatter.RecordResult(resultInfo);

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(2, lines.Length);

		var evt = TestResultEvent.Parse(lines[1]);
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

		var resultInfo = TestHelpers.CreateTestResult(
			"Ns.Class.FailTest", "test.dll", TestResultStatus.Failed,
			TimeSpan.FromMilliseconds(50),
			output: "console output",
			errorMessage: "Assert.Equal failed",
			errorStackTrace: "at Ns.Class.FailTest()");

		formatter.RecordResult(resultInfo);

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var evt = TestResultEvent.Parse(lines[1]);

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

		var resultInfo = TestHelpers.CreateTestResult(
			"Ns.Class.SkipTest", "test.dll", TestResultStatus.Skipped,
			TimeSpan.Zero, skipReason: "Not implemented yet");

		formatter.RecordResult(resultInfo);

		var lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var evt = TestResultEvent.Parse(lines[1]);

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

		var evt = TestResultEvent.Parse(lines[1]);
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
		formatter.RecordResult(TestHelpers.CreateTestResult("Test1", "a.dll", TestResultStatus.Passed, TimeSpan.FromSeconds(1)));
		formatter.RecordResult(TestHelpers.CreateTestResult("Test2", "a.dll", TestResultStatus.Failed, TimeSpan.FromSeconds(2), errorMessage: "boom"));
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
}
