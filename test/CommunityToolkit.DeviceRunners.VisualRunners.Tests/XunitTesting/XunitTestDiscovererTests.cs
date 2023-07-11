using CommunityToolkit.DeviceRunners;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.Xunit;

using Xunit;

namespace VisualRunnerTests.XunitTesting;

public class XunitTestDiscovererTests
{
	[Fact]
	public async Task DiscoverAsyncCanFindAllTests()
	{
		var assemblies = new[] { typeof(TestProject.Tests.XunitTests).Assembly };
		var options = new VisualTestRunnerConfiguration(assemblies);

		ITestDiscoverer discoverer = new XunitTestDiscoverer(options);

		var testAssemblies = await discoverer.DiscoverAsync();
		Assert.NotNull(testAssemblies);
		Assert.NotEmpty(testAssemblies);

		var assemblyInfo = Assert.Single(testAssemblies);
		Assert.NotNull(assemblyInfo);

		var testCases = assemblyInfo.TestCases;
		Assert.NotNull(testCases);
		Assert.NotEmpty(testCases);
		Assert.Equal(4, testCases.Count);
	}
}
