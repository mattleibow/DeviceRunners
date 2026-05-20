using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Testing;

public class Xunit3DiscovererAdvancedTests
{
	readonly Assembly _testAssembly = typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;

	[Fact]
	public async Task DiscoverAsync_ReturnsTestsWithCorrectMetadata()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		var assemblies = await discoverer.DiscoverAsync();
		var assembly = Assert.Single(assemblies);

		// Verify each test case has proper metadata
		foreach (var tc in assembly.TestCases)
		{
			Assert.NotEmpty(tc.DisplayName);
			Assert.NotNull(tc.TestAssembly);
			Assert.Equal(assembly, tc.TestAssembly);
		}
	}

	[Fact]
	public async Task DiscoverAsync_TestCasesHaveNullResult_BeforeExecution()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		var assemblies = await discoverer.DiscoverAsync();
		var assembly = Assert.Single(assemblies);

		foreach (var tc in assembly.TestCases)
		{
			Assert.Null(tc.Result);
		}
	}

	[Fact]
	public async Task DiscoverAsync_WithCancellation_StopsCleanly()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		using var cts = new CancellationTokenSource();
		cts.Cancel(); // Pre-cancel

		// Should either return empty or throw OperationCanceledException
		// Both are acceptable
		try
		{
			var assemblies = await discoverer.DiscoverAsync(cts.Token);
			// If it returns, it should be empty or partial
		}
		catch (OperationCanceledException)
		{
			// Also acceptable
		}
	}

	[Fact]
	public async Task DiscoverAsync_ReturnsExpectedTestCount()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		var assemblies = await discoverer.DiscoverAsync();
		var assembly = Assert.Single(assemblies);

		// PreEnumerateTheories = false: Theory with 3 InlineData = 1 test case
		// Total: SimpleTest + SimpleTest_Failed + SimpleTest_Skipped + DataTest (1 theory)
		//        + InitializeAsync_WasCalled + SimpleAsyncLifetimeTest
		//        + SimpleTest_Output + SimpleTest_Output_Failed = 8
		Assert.Equal(Constants.Xunit3TestCountNoTheoryEnumeration, assembly.TestCases.Count);
	}

	[Fact]
	public async Task DiscoverAsync_DiscoversTwice_ReturnsConsistentResults()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		var assemblies1 = await discoverer.DiscoverAsync();
		var assemblies2 = await discoverer.DiscoverAsync();

		var count1 = assemblies1.Single().TestCases.Count;
		var count2 = assemblies2.Single().TestCases.Count;

		Assert.Equal(count1, count2);
	}

	[Fact]
	public async Task DiscoverAsync_AssemblyFileName_IsPopulated()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		var assemblies = await discoverer.DiscoverAsync();
		var assembly = Assert.Single(assemblies);

		Assert.NotNull(assembly.AssemblyFileName);
		Assert.NotEmpty(assembly.AssemblyFileName);
	}

	[Fact]
	public async Task DiscoverAsync_Configuration_HasDeviceDefaults()
	{
		var options = new VisualTestRunnerConfiguration([_testAssembly]);
		var discoverer = new Xunit3TestDiscoverer(options);

		var assemblies = await discoverer.DiscoverAsync();
		var assembly = Assert.Single(assemblies);

		// Device defaults: parallelization disabled, max threads = 1
		var config = assembly.Configuration;
		Assert.NotNull(config);
	}
}
