namespace DeviceRunners.VisualRunners;

public static class TestRunnerExtensions
{
	public static Task RunTestsAsync(this ITestRunner runner, ITestAssemblyInfo testAssembly, CancellationToken cancellationToken = default) =>
		runner.RunTestsAsync(new[] { testAssembly }, cancellationToken);

	public static Task RunTestsAsync(this ITestRunner runner, ITestCaseInfo testCase, CancellationToken cancellationToken = default) =>
		runner.RunTestsAsync(new[] { testCase }, cancellationToken);
}
