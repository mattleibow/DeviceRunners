using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.Testing;

public class TestResultEventToInfoTests
{
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
}
