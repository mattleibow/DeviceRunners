using System.Reflection;

using DeviceRunners.VisualRunners.WebAssembly;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DeviceRunners.VisualRunners.NUnit;

/// <summary>
/// NUnit test runner plugin for browser WASM environments.
/// Discovers and executes tests, reporting results through <see cref="IResultChannel"/>.
/// </summary>
public class NUnitWasmTestRunnerPlugin : IWasmTestRunnerPlugin
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

			var assemblyFileName = !string.IsNullOrEmpty(assembly.Location)
				? assembly.Location
				: (assembly.GetName().Name + ".dll");

			var configuration = new Dictionary<string, object>();

			// Discover tests
			var builder = new DefaultTestAssemblyBuilder();
			var testAssembly = (TestAssembly)builder.Build(assembly, configuration);

			var assemblyInfo = new WasmNUnitAssemblyInfo(assemblyFileName);
			var testCaseMap = new Dictionary<ITest, WasmNUnitTestCaseInfo>();

			foreach (var test in GetLeafTests(testAssembly))
			{
				testCaseMap[test] = new WasmNUnitTestCaseInfo(assemblyInfo, test);
			}

			if (testCaseMap.Count == 0)
				continue;

			// Execute tests
			var listener = new WasmNUnitTestListener(testCaseMap, resultChannel);

			var wrappedBuilder = new PassThroughAssemblyBuilder(testAssembly);
			var runner = new NUnitTestAssemblyRunner(wrappedBuilder);
			runner.Load(assemblyFileName, configuration);

			runner.Run(listener, TestFilter.Empty);

			total += listener.Total;
			passed += listener.Passed;
			failed += listener.Failed;
			skipped += listener.Skipped;
		}

		return Task.FromResult(new WasmTestRunResult
		{
			TotalTests = total,
			PassedTests = passed,
			FailedTests = failed,
			SkippedTests = skipped
		});
	}

	static IEnumerable<ITest> GetLeafTests(ITest test)
	{
		if (!test.HasChildren && !test.IsSuite)
			yield return test;
		else if (test.Tests is not null)
			foreach (var child in test.Tests)
				foreach (var leaf in GetLeafTests(child))
					yield return leaf;
	}
}
