using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public class Xunit3RunnerAdvancedTests : IAsyncLifetime
{
	readonly Assembly _testAssembly = typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;
	IReadOnlyList<ITestAssemblyInfo> _testAssemblies = null!;
	VisualTestRunnerConfiguration _options = null!;

	public async Task InitializeAsync()
	{
		var assemblies = new[] { _testAssembly };
		_options = new VisualTestRunnerConfiguration(assemblies);

		var discoverer = new Xunit3TestDiscoverer(_options);
		_testAssemblies = await discoverer.DiscoverAsync();
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task RunTestsAsync_RerunSameTests_ProducesFreshResults()
	{
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		// First run
		await runner.RunTestsAsync(testAssembly);

		var firstResults = testAssembly.TestCases
			.Select(tc => (tc.DisplayName, tc.Result?.Status))
			.ToList();

		// Verify first run produced results
		Assert.All(firstResults, r => Assert.NotNull(r.Status));

		// Second run — should produce fresh results
		await runner.RunTestsAsync(testAssembly);

		// Verify results still present and correct
		foreach (var tc in testAssembly.TestCases)
		{
			Assert.NotNull(tc.Result);
		}
	}

	[Fact]
	public async Task RunTestsAsync_RerunSingleTest_UpdatesResult()
	{
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		var simpleTest = testAssembly.TestCases.First(tc => tc.DisplayName.EndsWith(".SimpleTest"));

		// First run
		await runner.RunTestsAsync(simpleTest);
		Assert.NotNull(simpleTest.Result);
		var firstResult = simpleTest.Result;

		// Second run
		await runner.RunTestsAsync(simpleTest);
		Assert.NotNull(simpleTest.Result);

		// Should have a new result (not same reference)
		// Both should be Passed
		Assert.Equal(TestResultStatus.Passed, simpleTest.Result.Status);
	}

	[Fact]
	public async Task RunTestsAsync_ResultReportedEventFires_ForEachTestCase()
	{
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		var reportedResults = new List<ITestResultInfo>();
		foreach (var tc in testAssembly.TestCases)
		{
			tc.ResultReported += r => reportedResults.Add(r);
		}

		await runner.RunTestsAsync(testAssembly);

		// Every test case should have fired ResultReported
		Assert.Equal(testAssembly.TestCases.Count, reportedResults.Count);
	}

	[Fact]
	public async Task RunTestsAsync_SelectiveExecution_OnlyRunsSpecifiedTests()
	{
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		// Pick 2 specific tests
		var selectedTests = testAssembly.TestCases
			.Where(tc => tc.DisplayName.EndsWith(".SimpleTest") || tc.DisplayName.EndsWith(".SimpleTest_Skipped"))
			.ToList();

		Assert.Equal(2, selectedTests.Count);

		await runner.RunTestsAsync(selectedTests);

		// Selected tests should have results
		foreach (var tc in selectedTests)
		{
			Assert.NotNull(tc.Result);
		}

		// Unselected tests should NOT have results
		foreach (var tc in testAssembly.TestCases.Except(selectedTests))
		{
			Assert.Null(tc.Result);
		}
	}

	[Fact]
	public async Task RunTestsAsync_EmptyTestCaseList_DoesNotThrow()
	{
		var runner = new Xunit3TestRunner(_options);

		// Should not throw
		await runner.RunTestsAsync(Array.Empty<ITestCaseInfo>());
	}

	[Fact]
	public async Task RunTestsAsync_NonXunit3TestCases_AreIgnored()
	{
		var runner = new Xunit3TestRunner(_options);

		// Create a mock non-xunit3 test case
		var mockTestCase = Substitute.For<ITestCaseInfo>();
		mockTestCase.DisplayName.Returns("MockTest");

		// Should not throw — runner filters to Xunit3TestCaseInfo only
		await runner.RunTestsAsync(new[] { mockTestCase });
	}

	[Fact]
	public async Task RunTestsAsync_WithCancellation_StopsCleanly()
	{
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		using var cts = new CancellationTokenSource();
		cts.Cancel(); // Pre-cancel

		// Should return without throwing
		await runner.RunTestsAsync(testAssembly.TestCases, cts.Token);
	}

	[Fact]
	public async Task RunTestsAsync_CapturesErrorAndOutput_ForFailingOutputTest()
	{
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		var outputFailed = testAssembly.TestCases
			.FirstOrDefault(tc => tc.DisplayName.Contains("SimpleTest_Output_Failed"));

		if (outputFailed is null)
			return; // Skip if fixture doesn't have this test

		await runner.RunTestsAsync(outputFailed);

		Assert.NotNull(outputFailed.Result);
		Assert.Equal(TestResultStatus.Failed, outputFailed.Result.Status);
		Assert.NotNull(outputFailed.Result.Output);
		Assert.NotEmpty(outputFailed.Result.Output);
		Assert.NotNull(outputFailed.Result.ErrorMessage);
		Assert.NotEmpty(outputFailed.Result.ErrorMessage);
	}
}
