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

	public virtual string SingleClassName => "TestProject.Tests.XunitTests";

	[Fact]
	public async Task DiscoveredTestCasesExposeClassAndMethodMetadata()
	{
		var assemblies = new[] { TestAssembly };
		var options = new VisualTestRunnerConfiguration(assemblies);
		var discoverer = CreateTestDiscoverer(options);
		var runner = CreateTestRunner(options);
		var channels = new DefaultResultChannelManager();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);
		await vm.StartAssemblyScanAsync();

		var cases = vm.TestAssemblies
			.SelectMany(a => a.TestAssemblyInfo.TestCases)
			.ToList();

		var simpleTest = Assert.Single(
			cases,
			c => c.TestClassName == SingleClassName && c.TestMethodName == "SimpleTest");

		Assert.Equal(SingleClassName, simpleTest.TestClassName);
		Assert.Equal("SimpleTest", simpleTest.TestMethodName);
		Assert.Equal("TestProject.Tests", simpleTest.TestClassNamespace);
	}

	[Fact]
	public async Task AutoStartWithFilterRunsOnlyMatchingTests()
	{
		var assemblies = new[] { TestAssembly };
		var filter = $"FullyQualifiedName={SingleClassName}.SimpleTest";
		var options = new VisualTestRunnerConfiguration(assemblies, autoStart: true, testCaseFilter: filter);
		var discoverer = CreateTestDiscoverer(options);
		var runner = CreateTestRunner(options);
		var channels = new DefaultResultChannelManager();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);
		await vm.StartAssemblyScanAsync();

		var allCases = vm.TestAssemblies.SelectMany(a => a.TestCases).ToList();

		var ran = allCases.Where(c => c.ResultStatus != TestResultStatus.NotRun).ToList();
		Assert.All(ran, c => Assert.Contains("SimpleTest", c.TestCaseInfo.TestMethodName));

		var dataTest = allCases.Where(c => c.TestCaseInfo.TestMethodName == "DataTest");
		Assert.All(dataTest, c => Assert.Equal(TestResultStatus.NotRun, c.ResultStatus));
	}

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
