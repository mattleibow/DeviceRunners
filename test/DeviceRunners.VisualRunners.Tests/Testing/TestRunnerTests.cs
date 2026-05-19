using DeviceRunners;
using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.Testing;

public abstract class TestRunnerTests : IAsyncLifetime
{
	public abstract ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration);

	public abstract ITestRunner CreateTestRunner(VisualTestRunnerConfiguration configuration);

	protected IReadOnlyList<ITestAssemblyInfo> _testAssemblies = null!;
	protected VisualTestRunnerConfiguration _options = null!;

	public async Task InitializeAsync()
	{
		var assemblies = new[] { typeof(TestProject.Tests.XunitTests).Assembly };
		_options = new VisualTestRunnerConfiguration(assemblies);

		var discoverer = CreateTestDiscoverer(_options);

		_testAssemblies = await discoverer.DiscoverAsync();
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task RunTestsAsyncCanRunEntireAssembly()
	{
		var testAssembly = _testAssemblies[0];

		var runner = CreateTestRunner(_options);
		await runner.RunTestsAsync(testAssembly);

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

		var runner = CreateTestRunner(_options);
		await runner.RunTestsAsync(simpleTest);

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
		var simpleTests = testAssembly.TestCases.Where(tc => tc.DisplayName.Contains("_Output")).ToList();

		var runner = CreateTestRunner(_options);
		await runner.RunTestsAsync(simpleTests);

		foreach (var test in testAssembly.TestCases.Except(simpleTests))
		{
			Assert.Null(test.Result);
		}

		foreach (var test in simpleTests)
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
			Assert.Equal(Constants.TestOutput, output);
		else
			Assert.Empty(output);
	
		if (name.EndsWith("_Failed"))
		{
			Assert.True(TestResultStatus.Failed == status, $"'{name}' should have failed but instead {status}.");

			Assert.Empty(skip);
			Assert.Contains(Constants.ErrorMessage, error);
			Assert.NotEmpty(stacktrace);
		}
		else if (name.EndsWith("_Skipped"))
		{
			Assert.True(TestResultStatus.Skipped == status, $"'{name}' should have been skipped but instead {status}.");

			Assert.Equal(Constants.SkippedReason, skip);
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
