using System.Reflection;

using DeviceRunners;
using DeviceRunners.Core;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Xunit3.Testing;

public class Xunit3HomeViewModelTests
{
	static readonly Assembly TestAssembly = typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;
	const int ExpectedTestCount = 8;

	[Fact]
	public async Task StartAssemblyScanAsyncCreatesAllTheExpectedViewModels()
	{
		var assemblies = new[] { TestAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies);
		var discoverer = new Xunit3TestDiscoverer(options);
		var runner = new Xunit3TestRunner(options);
		var channels = new DefaultResultChannelManager();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);

		await vm.StartAssemblyScanAsync(TestContext.Current.CancellationToken);

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
		var discoverer = new Xunit3TestDiscoverer(options);
		var runner = new Xunit3TestRunner(options);
		var channels = new DefaultResultChannelManager();

		var terminator = Substitute.For<IAppTerminator>();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels, terminator);

		await vm.StartAssemblyScanAsync(TestContext.Current.CancellationToken);

		if (autoStart)
			Assert.NotEqual(TestResultStatus.NotRun, vm.TestAssemblies[0].ResultStatus);
		else
			Assert.Equal(TestResultStatus.NotRun, vm.TestAssemblies[0].ResultStatus);

		if (autoStart && autoTerminate)
			terminator.Received().Terminate();
		else
			terminator.DidNotReceive().Terminate();
	}

	[Fact]
	public async Task DiscoveredTestCasesExposeClassAndMethodMetadata()
	{
		var assemblies = new[] { TestAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies);
		var discoverer = new Xunit3TestDiscoverer(options);
		var runner = new Xunit3TestRunner(options);
		var channels = new DefaultResultChannelManager();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);
		await vm.StartAssemblyScanAsync(TestContext.Current.CancellationToken);

		var cases = vm.TestAssemblies
			.SelectMany(a => a.TestAssemblyInfo.TestCases)
			.ToList();

		var simpleTest = Assert.Single(
			cases,
			c => c.TestClassName == "TestProject.Xunit3Tests.Xunit3Tests" && c.TestMethodName == "SimpleTest");

		Assert.Equal("TestProject.Xunit3Tests", simpleTest.TestClassNamespace);
	}

	[Fact]
	public async Task AutoStartWithFilterRunsOnlyMatchingTests()
	{
		var assemblies = new[] { TestAssembly };
		var filter = "FullyQualifiedName=TestProject.Xunit3Tests.Xunit3Tests.SimpleTest";
		var options = new VisualTestRunnerConfiguration(assemblies, autoStart: true, testCaseFilter: filter);
		var discoverer = new Xunit3TestDiscoverer(options);
		var runner = new Xunit3TestRunner(options);
		var channels = new DefaultResultChannelManager();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);
		await vm.StartAssemblyScanAsync(TestContext.Current.CancellationToken);

		var allCases = vm.TestAssemblies.SelectMany(a => a.TestCases).ToList();

		var ran = allCases.Where(c => c.ResultStatus != TestResultStatus.NotRun).ToList();
		Assert.All(ran, c => Assert.Contains("SimpleTest", c.TestCaseInfo.TestMethodName));

		var dataTest = allCases.Where(c => c.TestCaseInfo.TestMethodName == "DataTest");
		Assert.All(dataTest, c => Assert.Equal(TestResultStatus.NotRun, c.ResultStatus));
	}
}
