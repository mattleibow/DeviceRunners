using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.Testing;

public abstract class TestDiscovererTests
{
	public abstract ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration);

	protected virtual Assembly TestAssembly => typeof(TestProject.Tests.XunitTests).Assembly;

	protected virtual int ExpectedTestCount => Constants.TestCount;

	[Fact]
	public async Task DiscoverAsyncCanFindAllTests()
	{
		var assemblies = new[] { TestAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies);

		ITestDiscoverer discoverer = CreateTestDiscoverer(options);

		var testAssemblies = await discoverer.DiscoverAsync();
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
