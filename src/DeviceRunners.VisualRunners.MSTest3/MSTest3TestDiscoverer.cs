using Microsoft.Extensions.Logging;

using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;

namespace DeviceRunners.VisualRunners.MSTest3;

public class MSTest3TestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<System.Reflection.Assembly> _testAssemblies;

	public MSTest3TestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<MSTest3TestDiscoverer>? logger = null)
	{
		_diagnosticsManager = diagnosticsManager;
		_testAssemblies = options.TestAssemblies.ToArray();
	}

	public Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default) =>
		AsyncUtils.RunAsync(() => Discover(cancellationToken));

	IReadOnlyList<ITestAssemblyInfo> Discover(CancellationToken cancellationToken = default)
	{
		var result = new List<ITestAssemblyInfo>();

		try
		{
			var context = new VsTestAdapterContext();
			var logger = new VsTestMessageLogger(_diagnosticsManager);

			foreach (var assm in _testAssemblies)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				var assemblyFileName = FileSystemUtils.GetAssemblyFileName(assm);

				try
				{
					var sink = new VsTestDiscoverySink();
					var discoverer = new MSTestDiscoverer();
					discoverer.DiscoverTests(new[] { assemblyFileName }, context, logger, sink);

					var testAssembly = new MSTest3TestAssemblyInfo(assemblyFileName);
					var testCases = sink.TestCases
						.Select(tc => new MSTest3TestCaseInfo(testAssembly, tc))
						.ToList();

					if (testCases.Count > 0)
					{
						testAssembly.TestCases.AddRange(testCases);
						result.Add(testAssembly);
					}
				}
				catch (Exception ex)
				{
					_diagnosticsManager?.PostDiagnosticMessage($"Exception discovering tests in assembly '{assemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
				}
			}
		}
		catch (Exception ex)
		{
			_diagnosticsManager?.PostDiagnosticMessage($"Exception discovering tests: '{ex.Message}'{Environment.NewLine}{ex}");
		}

		return result;
	}
}
