using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using Xunit;

namespace VisualRunnerTests.Xunit3.Testing;

public class Xunit3TestRunnerTests : IAsyncLifetime
{
	static readonly Assembly TestAssembly = typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;

	IReadOnlyList<ITestAssemblyInfo> _testAssemblies = null!;
	VisualTestRunnerConfiguration _options = null!;

	public async ValueTask InitializeAsync()
	{
		var assemblies = new[] { TestAssembly };
		_options = new VisualTestRunnerConfiguration(assemblies);

		var discoverer = new Xunit3TestDiscoverer(_options);
		_testAssemblies = await discoverer.DiscoverAsync(TestContext.Current.CancellationToken);
	}

	public ValueTask DisposeAsync() => default;

	[Fact]
	public async Task RunTestsAsyncCanRunEntireAssembly()
	{
		var testAssembly = _testAssemblies[0];

		var runner = new Xunit3TestRunner(_options);
		await runner.RunTestsAsync(testAssembly, TestContext.Current.CancellationToken);

		foreach (var test in testAssembly.TestCases)
		{
			Assert.NotNull(test.Result);
			AssertTestResult(test);
		}
	}

	[Fact]
	public async Task RunTestsAsyncCanRunSingleTestCase()
	{
		var testAssembly = _testAssemblies[0];
		var simpleTest = testAssembly.TestCases.Single(tc => tc.DisplayName.EndsWith(".SimpleTest"));

		var runner = new Xunit3TestRunner(_options);
		await runner.RunTestsAsync(simpleTest, TestContext.Current.CancellationToken);

		foreach (var test in testAssembly.TestCases.Except(new[] { simpleTest }))
		{
			Assert.Null(test.Result);
		}

		Assert.NotNull(simpleTest.Result);
		AssertTestResult(simpleTest);
	}

	[Fact]
	public async Task RunTestsAsyncCanCaptureOutput()
	{
		var testAssembly = _testAssemblies[0];
		var outputTests = testAssembly.TestCases.Where(tc => tc.DisplayName.Contains("_Output")).ToList();

		Assert.NotEmpty(outputTests);

		var runner = new Xunit3TestRunner(_options);
		await runner.RunTestsAsync(outputTests, TestContext.Current.CancellationToken);

		foreach (var test in testAssembly.TestCases.Except(outputTests))
		{
			Assert.Null(test.Result);
		}

		foreach (var test in outputTests)
		{
			Assert.NotNull(test.Result);
			AssertTestResult(test);
		}
	}

	static void AssertTestResult(ITestCaseInfo test)
	{
		var name = test.DisplayName;
		var status = test.Result?.Status ?? TestResultStatus.NotRun;
		var output = test.Result?.Output?.Trim() ?? "";
		var skip = test.Result?.SkipReason ?? "";
		var error = test.Result?.ErrorMessage ?? "";
		var stacktrace = test.Result?.ErrorStackTrace ?? "";

		if (name.Contains("_Output"))
			Assert.Equal(TestProject.Xunit3Tests.Constants.TestOutput, output);
		else
			Assert.Empty(output);

		if (name.EndsWith("_Failed"))
		{
			Assert.True(TestResultStatus.Failed == status, $"'{name}' should have failed but instead {status}.");
			Assert.Empty(skip);
			Assert.Contains(TestProject.Xunit3Tests.Constants.ErrorMessage, error);
			Assert.NotEmpty(stacktrace);
		}
		else if (name.EndsWith("_Skipped"))
		{
			Assert.True(TestResultStatus.Skipped == status, $"'{name}' should have been skipped but instead {status}.");
			Assert.Equal(TestProject.Xunit3Tests.Constants.SkippedReason, skip);
			Assert.Empty(error);
			Assert.Empty(stacktrace);
		}
		else
		{
			Assert.True(TestResultStatus.Passed == status, $"'{name}' should have passed but instead {status}.");
			Assert.Empty(skip);
			Assert.Empty(error);
			Assert.Empty(stacktrace);
		}
	}
}
