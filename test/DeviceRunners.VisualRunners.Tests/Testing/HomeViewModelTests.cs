using DeviceRunners;
using DeviceRunners.Core;
using DeviceRunners.VisualRunners;

using NSubstitute;

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

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public async Task AutoStartAndAutoTerminateWorkCorrectly(bool autoStart, bool autoTerminate)
	{
		var assemblies = new[] { typeof(TestProject.Tests.XunitTests).Assembly };
		var options = new VisualTestRunnerConfiguration(assemblies, autoStart, autoTerminate);
		var discoverer = CreateTestDiscoverer(options);
		var runner = CreateTestRunner();
		
		var terminator = Substitute.For<IAppTerminator>();

		var vm = new HomeViewModel(new[] { discoverer }, new[] { runner }, options, terminator);

		await vm.StartAssemblyScanAsync();

		if (autoStart)
			Assert.NotEqual(TestResultStatus.NotRun, vm.TestAssemblies[0].ResultStatus);
		else
			Assert.Equal(TestResultStatus.NotRun, vm.TestAssemblies[0].ResultStatus);
		
		if (autoStart && autoTerminate)
			terminator.Received().Terminate();
		else
			terminator.DidNotReceive().Terminate();
	}
}
