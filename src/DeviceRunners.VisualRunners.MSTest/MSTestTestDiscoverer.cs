using System.Reflection;

using Microsoft.Extensions.Logging;

namespace DeviceRunners.VisualRunners.MSTest;

public class MSTestTestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<Assembly> _testAssemblies;

	public MSTestTestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<MSTestTestDiscoverer>? logger = null)
	{
		_diagnosticsManager = diagnosticsManager;
		_testAssemblies = options.TestAssemblies.ToArray();
	}

	public async Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
	{
		var result = new List<ITestAssemblyInfo>();

		foreach (var assm in _testAssemblies)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			// Use the logical name (not the file path) so discovery works on platforms
			// without filesystem-backed assemblies such as WASM.
			var assemblyFileName = assm.GetName().Name + ".dll";

			try
			{
				var testAssembly = new MSTestTestAssemblyInfo(assemblyFileName, assm);
				var testCases = new List<MSTestTestCaseInfo>();

				void OnNode(WireTestNode node)
				{
					// A discovery request reports each test as an 'action' node in the 'discovered'
					// state, without executing anything.
					if (node.IsAction && node.IsDiscovered)
						testCases.Add(MSTestTestCaseInfo.FromDiscoveredNode(testAssembly, node));
				}

				await MSTestServerModeHost.RunSessionAsync(
					assm,
					MSTestServerModeHost.DiscoverTestsMethod,
					tests: null,
					OnNode,
					cancellationToken);

				if (testCases.Count > 0)
				{
					testAssembly.TestCases.AddRange(testCases);
					result.Add(testAssembly);
				}
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				_diagnosticsManager?.PostDiagnosticMessage($"Exception discovering tests in assembly '{assemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
			}
		}

		return result;
	}
}
