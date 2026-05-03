using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public class TestResultEventRoundTripTests
{
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
	public void RoundTrip_ToStringThenParse_PreservesAllFields()
	{
		var original = TestResultEvent.FromInfo(
			TestHelpers.CreateTestResult(
				"Ns.Class.Method(a: 1)", "test.dll", TestResultStatus.Failed,
				TimeSpan.FromMilliseconds(500),
				output: "stdout text",
				errorMessage: "Expected true",
				errorStackTrace: "at line 42"));

		var json = original.ToString();
		var parsed = TestResultEvent.Parse(json);

		Assert.NotNull(parsed);
		Assert.Equal(original.Type, parsed.Type);
		Assert.Equal(original.DisplayName, parsed.DisplayName);
		Assert.Equal(original.Assembly, parsed.Assembly);
		Assert.Equal(original.Status, parsed.Status);
		Assert.Equal(original.Duration, parsed.Duration);
		Assert.Equal(original.Output, parsed.Output);
		Assert.Equal(original.ErrorMessage, parsed.ErrorMessage);
		Assert.Equal(original.ErrorStackTrace, parsed.ErrorStackTrace);
	}
}
