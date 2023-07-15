using System.Reflection;

using Microsoft.Extensions.Logging;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

public class NUnitTestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<Assembly> _testAssemblies;

	public NUnitTestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<NUnitTestDiscoverer>? logger = null)
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
			foreach (var assm in _testAssemblies)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				var assemblyFileName = FileSystemUtils.GetAssemblyFileName(assm);

				// TODO: configuration
				var configuration = new Dictionary<string, object>();

				try
				{
					var builder = new DefaultTestAssemblyBuilder();
					var nunitTestAssembly = (TestAssembly)builder.Build(assm, configuration);

					var testAssembly = new NUnitTestAssemblyInfo(assemblyFileName, nunitTestAssembly, configuration);
					var testCases = GetChildTests(nunitTestAssembly)
						.Select(tc => new NUnitTestCaseInfo(testAssembly, tc))
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

	static IEnumerable<ITest> GetChildTests(ITest test)
	{
		if (!test.HasChildren && !test.IsSuite)
			yield return test;
		else
			foreach (var child in test.Tests)
				foreach (var ct in GetChildTests(child))
					yield return ct;
	}
}
