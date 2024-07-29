using DeviceRunners.VisualRunners;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.ViewModelTesting;

public class HomeViewModelTests
{
	[Fact]
	public async Task StartAssemblyScanAsyncCreatesAllTheViewExpectedModels()
	{
		var inputAssembly = Substitute.For<ITestAssemblyInfo>();

		var inputTestCase = Substitute.For<ITestCaseInfo>();
		inputTestCase.TestAssembly.Returns(inputAssembly);
		inputTestCase.DisplayName.Returns("Substitute.Tests.TestMethod");

		inputAssembly.AssemblyFileName.Returns("Substitute.Tests.dll");
		inputAssembly.TestCases.Returns(new[] { inputTestCase });

		var options = new VisualTestRunnerConfiguration([]);

		var expectedTestAssemblies = new List<ITestAssemblyInfo> { inputAssembly };
		var discoverer = Substitute.For<ITestDiscoverer>();
		discoverer.DiscoverAsync().Returns(Task.FromResult<IReadOnlyList<ITestAssemblyInfo>>(expectedTestAssemblies));

		var runner = Substitute.For<ITestRunner>();
		var channels = Substitute.For<IResultChannelManager>();

		var vm = new HomeViewModel(options, [discoverer], [runner], channels);
		Assert.Empty(vm.TestAssemblies);

		await vm.StartAssemblyScanAsync();

		var vmAssembly = Assert.Single(vm.TestAssemblies);
		Assert.Equal(inputAssembly, vmAssembly.TestAssemblyInfo);
		Assert.Equal(TestResultStatus.NotRun, vmAssembly.ResultStatus);

		var vmTest = Assert.Single(vmAssembly.TestCases);
		Assert.Equal(inputTestCase, vmTest.TestCaseInfo);
		Assert.NotNull(vmTest.TestResult);
		Assert.Equal(TestResultStatus.NotRun, vmTest.ResultStatus);
		Assert.Equal("Substitute.Tests.TestMethod", vmTest.DisplayName);

		var vmResult = vmTest.TestResult;
		Assert.Equal(vmTest, vmResult.TestCase);
		Assert.Null(vmResult.TestResultInfo);
		Assert.Equal(TestResultStatus.NotRun, vmResult.ResultStatus);
	}
}
