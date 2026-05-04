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

	[Fact]
	public void RoundTrip_NonAsciiTestName_PreservesUnicodeCharacters()
	{
		// Verifies that System.Text.Json escapes non-ASCII to \uXXXX sequences
		// (producing pure-ASCII JSON lines), and that Parse correctly reconstructs them.
		var original = TestResultEvent.FromInfo(
			TestHelpers.CreateTestResult(
				"Ns.\u30c6\u30b9\u30c8.Method_\u00e9\U0001f389(x: \"\u4e16\u754c\")", "test-\u00fc.dll",
				TestResultStatus.Passed, TimeSpan.FromMilliseconds(100),
				output: "Output with \u2603 snowman"));

		var json = original.ToString();

		// The serialized JSON line must be pure ASCII (all non-ASCII escaped)
		Assert.All(json, c => Assert.True(c < 128, $"Non-ASCII char U+{(int)c:X4} found in JSON line"));

		// Round-trip must reconstruct the original Unicode strings
		var parsed = TestResultEvent.Parse(json);
		Assert.NotNull(parsed);
		Assert.Equal(original.DisplayName, parsed.DisplayName);
		Assert.Equal(original.Assembly, parsed.Assembly);
		Assert.Equal(original.Output, parsed.Output);

		// Full ToInfo round-trip
		var info = parsed.ToInfo();
		Assert.Equal("Ns.\u30c6\u30b9\u30c8.Method_\u00e9\U0001f389(x: \"\u4e16\u754c\")", info.TestCase.DisplayName);
		Assert.Equal("Output with \u2603 snowman", info.Output);
	}
}
