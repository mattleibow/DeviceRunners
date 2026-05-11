using System.Reflection;

using DeviceRunners.VisualRunners.WebAssembly;

using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Xunit v2 test runner plugin for browser WASM environments.
/// Discovers and executes tests, reporting results through <see cref="IResultChannel"/>.
/// </summary>
public class XunitWasmTestRunnerPlugin : IWasmTestRunnerPlugin
{
	public Task<WasmTestRunResult> RunTestsAsync(
		IEnumerable<Assembly> assemblies,
		IResultChannel resultChannel,
		CancellationToken cancellationToken = default)
	{
		int total = 0, passed = 0, failed = 0, skipped = 0;

		foreach (var assembly in assemblies)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			var assemblyFileName = GetAssemblyFileName(assembly);
			var configuration = new TestAssemblyConfiguration();
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);
			var executionOptions = TestFrameworkOptions.ForExecution(configuration);

			using var controller = new XunitFrontController(
				AppDomainSupport.Denied, assemblyFileName, null, false);

			// Discover tests
			using var discoverySink = new TestDiscoverySink(() => cancellationToken.IsCancellationRequested);
			controller.Find(false, discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();

			if (discoverySink.TestCases.Count == 0)
				continue;

			// Build wrapper types for result reporting
			var assemblyInfo = new WasmXunitAssemblyInfo(assemblyFileName);
			var testCaseMap = new Dictionary<ITestCase, WasmXunitTestCaseInfo>();
			foreach (var tc in discoverySink.TestCases)
			{
				testCaseMap[tc] = new WasmXunitTestCaseInfo(assemblyInfo, tc);
			}

			// Execute tests — RunTests blocks until complete
			var executionSink = new WasmXunitExecutionSink(testCaseMap, resultChannel);

			controller.RunTests(
				discoverySink.TestCases.ToList(),
				executionSink,
				executionOptions);

			total += executionSink.Total;
			passed += executionSink.Passed;
			failed += executionSink.Failed;
			skipped += executionSink.Skipped;
		}

		return Task.FromResult(new WasmTestRunResult
		{
			TotalTests = total,
			PassedTests = passed,
			FailedTests = failed,
			SkippedTests = skipped
		});
	}

	static string GetAssemblyFileName(Assembly assembly)
	{
		if (!string.IsNullOrEmpty(assembly.Location))
			return assembly.Location;

		// In browser WASM, Assembly.Location is empty
		return assembly.GetName().Name + ".dll";
	}
}
