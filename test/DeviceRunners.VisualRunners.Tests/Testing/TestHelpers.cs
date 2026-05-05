using DeviceRunners.VisualRunners;

using NSubstitute;

namespace VisualRunnerTests.Testing;

internal static class TestHelpers
{
	public static ITestResultInfo CreateTestResult(
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
