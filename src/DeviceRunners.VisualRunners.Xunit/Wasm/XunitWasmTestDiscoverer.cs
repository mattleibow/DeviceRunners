using System.Reflection;

using Xunit;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Xunit test discoverer for browser WASM environments.
/// Uses reflection to find [Fact] and [Theory] methods instead of
/// XunitFrontController, which requires filesystem access unavailable in WASM.
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

			try
			{
				var testAssembly = new XunitWasmTestAssemblyInfo(assemblyFileName);
				var testCases = new List<XunitWasmTestCaseInfo>();

				foreach (var type in assembly.GetExportedTypes())
				{
					if (type.IsAbstract || type.IsInterface)
						continue;

					foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
					{
						var factAttr = method.GetCustomAttribute<FactAttribute>();
						if (factAttr is null)
							continue;

						var inlineDataAttrs = method.GetCustomAttributes<InlineDataAttribute>().ToList();

						if (inlineDataAttrs.Count > 0)
						{
							foreach (var inlineData in inlineDataAttrs)
							{
								var data = inlineData.GetData(method).First();
								var displayName = $"{type.FullName}.{method.Name}({string.Join(", ", data.Select(d => d?.ToString() ?? "null"))})";
								testCases.Add(new XunitWasmTestCaseInfo(testAssembly, type, method, displayName, factAttr.Skip, data));
							}
						}
						else
						{
							var displayName = $"{type.FullName}.{method.Name}";
							testCases.Add(new XunitWasmTestCaseInfo(testAssembly, type, method, displayName, factAttr.Skip, null));
						}
					}
				}

				if (testCases.Count > 0)
				{
					testAssembly.SetTestCases(testCases);
					result.Add(testAssembly);
				}
			}
			catch (Exception ex)
			{
				_diagnosticsManager?.PostDiagnosticMessage($"Exception discovering tests in assembly '{assemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
			}
		}

		return Task.FromResult<IReadOnlyList<ITestAssemblyInfo>>(result);
	}
}
