using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public class TestResultEventFactoryTests
{
	[Fact]
	public void Begin_CreatesBeginEvent()
	{
		var evt = TestResultEvent.Begin("starting");

		Assert.Equal(TestResultEvent.TypeBegin, evt.Type);
		Assert.Equal("starting", evt.Message);
		Assert.NotNull(evt.Timestamp);
	}

	[Fact]
	public void Begin_WithoutMessage_OmitsMessage()
	{
		var evt = TestResultEvent.Begin();

		Assert.Equal(TestResultEvent.TypeBegin, evt.Type);
		Assert.Null(evt.Message);
	}

	[Fact]
	public void End_CreatesEndEvent()
	{
		var evt = TestResultEvent.End();

		Assert.Equal(TestResultEvent.TypeEnd, evt.Type);
		Assert.NotNull(evt.Timestamp);
	}

	[Fact]
	public void FromInfo_CreatesResultEvent()
	{
		var info = TestHelpers.CreateTestResult(
			"Ns.MyTest", "my.dll", TestResultStatus.Passed,
			TimeSpan.FromSeconds(1), output: "hello");

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
	public void FromInfo_FailedTest_IncludesErrorFields()
	{
		var info = TestHelpers.CreateTestResult(
			"Ns.FailTest", "test.dll", TestResultStatus.Failed,
			TimeSpan.FromMilliseconds(50),
			errorMessage: "Assert failed",
			errorStackTrace: "at Ns.FailTest()");

		var evt = TestResultEvent.FromInfo(info);

		Assert.Equal("Failed", evt.Status);
		Assert.Equal("Assert failed", evt.ErrorMessage);
		Assert.Equal("at Ns.FailTest()", evt.ErrorStackTrace);
	}

	[Fact]
	public void FromInfo_SkippedTest_IncludesSkipReason()
	{
		var info = TestHelpers.CreateTestResult(
			"Ns.SkipTest", "test.dll", TestResultStatus.Skipped,
			TimeSpan.Zero, skipReason: "Not ready");

		var evt = TestResultEvent.FromInfo(info);

		Assert.Equal("Skipped", evt.Status);
		Assert.Equal("Not ready", evt.SkipReason);
	}
}
