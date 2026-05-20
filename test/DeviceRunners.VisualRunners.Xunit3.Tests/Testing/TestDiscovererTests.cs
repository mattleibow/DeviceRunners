using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using Xunit;

namespace VisualRunnerTests.Xunit3.Testing;

public class Xunit3TestDiscovererTests
{
	readonly Assembly _testAssembly = typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;

	// PreEnumerateTheories = false: Theory with 3 InlineData = 1 test case
	// Total: SimpleTest + SimpleTest_Failed + SimpleTest_Skipped + DataTest (1 theory)
	//        + InitializeAsync_WasCalled + SimpleAsyncLifetimeTest
	//        + SimpleTest_Output + SimpleTest_Output_Failed = 8
	const int ExpectedTestCount = 8;

	[Fact]
	public async Task DiscoverAsyncCanFindAllTests()
	{
		var assemblies = new[] { _testAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies);

		ITestDiscoverer discoverer = new Xunit3TestDiscoverer(options);

		var testAssemblies = await discoverer.DiscoverAsync(TestContext.Current.CancellationToken);
		Assert.NotNull(testAssemblies);
		Assert.NotEmpty(testAssemblies);

		var assemblyInfo = Assert.Single(testAssemblies);
		Assert.NotNull(assemblyInfo);

		var testCases = assemblyInfo.TestCases;
		Assert.NotNull(testCases);
		Assert.NotEmpty(testCases);
		Assert.Equal(ExpectedTestCount, testCases.Count);

		foreach (var test in testCases)
		{
			Assert.Null(test.Result);
		}
	}
}
