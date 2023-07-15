using CommunityToolkit.DeviceRunners;
using CommunityToolkit.DeviceRunners.VisualRunners;

using Xunit;

namespace VisualRunnerTests.Testing;

public abstract class HomeViewModelTests
{
	public abstract ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration);

	public abstract ITestRunner CreateTestRunner();

	[Fact]
	public async Task StartAssemblyScanAsyncCreatesAllTheViewExpectedModels()
	{
		var assemblies = new[] { typeof(TestProject.Tests.XunitTests).Assembly };
		var options = new VisualTestRunnerConfiguration(assemblies);
		var discoverer = CreateTestDiscoverer(options);
		var runner = CreateTestRunner();

		var vm = new HomeViewModel(new[] { discoverer }, new[] { runner });

		await vm.StartAssemblyScanAsync();

		var vmAssembly = Assert.Single(vm.TestAssemblies);
		Assert.NotEmpty(vmAssembly.TestCases);
		Assert.Equal(Constants.TestCount, vmAssembly.TestCases.Count);
	}
}
