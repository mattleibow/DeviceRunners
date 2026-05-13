using System.Reflection;

using Xunit;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Xunit test discoverer for browser WASM environments.
/// Uses xunit's own <see cref="XunitTestFrameworkDiscoverer"/> via
/// <see cref="WasmXunitDiscoverer"/> for proper discovery of all test types
/// (Fact, Theory, MemberData, ClassData, etc.) without spawning threads.
/// </summary>
public class XunitWasmTestDiscoverer : ITestDiscoverer
{
	readonly IReadOnlyList<Assembly> _testAssemblies;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public XunitWasmTestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null)
	{
		_testAssemblies = options.TestAssemblies.ToArray();
		_diagnosticsManager = diagnosticsManager;
	}

	public Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
	{
		var result = new List<ITestAssemblyInfo>();

		foreach (var assembly in _testAssemblies)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			var assemblyFileName = assembly.GetName().Name + ".dll";
			var configuration = new TestAssemblyConfiguration
			{
				ShadowCopy = false,
				ParallelizeAssembly = false,
				ParallelizeTestCollections = false,
				MaxParallelThreads = 1,
				PreEnumerateTheories = false,
			};
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);

			try
			{
				var assemblyInfo = new ReflectionAssemblyInfo(assembly);

				using var discoverer = new WasmXunitDiscoverer(
					assemblyInfo,
					NullSourceInformationProvider.Instance,
					NullMessageSink.Instance);

				var testCases = discoverer.DiscoverTests(discoveryOptions);

				if (testCases.Count > 0)
				{
					var testAssembly = new XunitWasmTestAssemblyInfo(assemblyFileName, configuration);
					testAssembly.TestCases.AddRange(
						testCases.Select(tc => new XunitWasmTestCaseInfo(testAssembly, tc)));
					result.Add(testAssembly);
				}
			}
			catch (Exception ex)
			{
				_diagnosticsManager?.PostDiagnosticMessage(
					$"Exception discovering tests in assembly '{assemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
			}
		}

		return Task.FromResult<IReadOnlyList<ITestAssemblyInfo>>(result);
	}
}
