using CommunityToolkit.DeviceRunners;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.Xunit;

using Xunit;

namespace VisualRunnerTests.XunitTesting;

public class XunitHomeViewModelTests
{
	[Fact]
	public async Task StartAssemblyScanAsyncCreatesAllTheViewExpectedModels()
	{
		var assemblies = new[] { typeof(TestProject.Tests.XunitTests).Assembly };
		var options = new VisualTestRunnerConfiguration(assemblies);
		var discoverer = new XunitTestDiscoverer(options);
		var runner = new XunitTestRunner();

		var vm = new HomeViewModel(discoverer, runner);

		await vm.StartAssemblyScanAsync();

		var vmAssembly = Assert.Single(vm.TestAssemblies);
		Assert.NotEmpty(vmAssembly.TestCases);
		Assert.Equal(4, vmAssembly.TestCases.Count);
	}
}
