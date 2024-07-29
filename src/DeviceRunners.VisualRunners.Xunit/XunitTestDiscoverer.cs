using System.Diagnostics;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

public class XunitTestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<Assembly> _testAssemblies;

	public XunitTestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<XunitTestDiscoverer>? logger = null)
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
				var assemblyFileName = FileSystemUtils.GetAssemblyFileName(assm);
				var configuration = GetConfiguration(Path.GetFileNameWithoutExtension(assemblyFileName));
				var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);

				if (cancellationToken.IsCancellationRequested)
					break;

				try
				{
					using var framework = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName, null, false);
					using var sink = new TestDiscoverySink(() => cancellationToken.IsCancellationRequested);
					framework.Find(false, sink, discoveryOptions);
					sink.Finished.WaitOne();

					var testAssembly = new XunitTestAssemblyInfo(assemblyFileName, configuration);
					var testCases = sink.TestCases
						.Select(tc => new XunitTestCaseInfo(testAssembly, tc))
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

	static TestAssemblyConfiguration GetConfiguration(string assemblyName)
	{
		using var stream = GetConfigurationStreamForAssembly(assemblyName);
		if (stream is not null)
			return ConfigReader.Load(stream);

		return new TestAssemblyConfiguration();
	}

	static Stream? GetConfigurationStreamForAssembly(string assemblyName)
	{
		var stream = FileSystemUtils.OpenAppPackageFile($"{assemblyName}.xunit.runner.json");
		if (stream is not null)
			return stream;

		stream = FileSystemUtils.OpenAppPackageFile($"xunit.runner.json");
		if (stream is not null)
			return stream;

		return null;
	}
}
