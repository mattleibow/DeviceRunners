using System.Reflection;
using System.Threading.Channels;

using DeviceRunners;
using DeviceRunners.Core;
using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public abstract class HomeViewModelTests
{
	public abstract ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration);

	public abstract ITestRunner CreateTestRunner(VisualTestRunnerConfiguration configuration);

	public virtual Assembly TestAssembly => typeof(TestProject.Tests.XunitTests).Assembly;

	public virtual int ExpectedTestCount => Constants.TestCount;

	[Fact]
	public async Task StartAssemblyScanAsyncCreatesAllTheViewExpectedModels()
	{
		var assemblies = new[] { TestAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies);
		var discoverer = CreateTestDiscoverer(options);
		var runner = CreateTestRunner(options);
		var channels = new DefaultResultChannelManager();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);

		await vm.StartAssemblyScanAsync();

		var vmAssembly = Assert.Single(vm.TestAssemblies);
		Assert.NotEmpty(vmAssembly.TestCases);
		Assert.Equal(ExpectedTestCount, vmAssembly.TestCases.Count);
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public async Task AutoStartAndAutoTerminateWorkCorrectly(bool autoStart, bool autoTerminate)
	{
		var assemblies = new[] { TestAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies, autoStart, autoTerminate);
		var discoverer = CreateTestDiscoverer(options);
		var runner = CreateTestRunner(options);
		var channels = new DefaultResultChannelManager();

		var terminator = Substitute.For<IAppTerminator>();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels, terminator);

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
