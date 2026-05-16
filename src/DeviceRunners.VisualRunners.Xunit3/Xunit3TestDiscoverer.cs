using System.Reflection;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

public class Xunit3TestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<Assembly> _testAssemblies;

	public Xunit3TestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<Xunit3TestDiscoverer>? logger = null)
	{
		_diagnosticsManager = diagnosticsManager;
		_testAssemblies = options.TestAssemblies.ToArray();
	}

	public async Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
	{
		var result = new List<ITestAssemblyInfo>();

		try
		{
			foreach (var assm in _testAssemblies)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				var assemblyFileName = FileSystemUtils.GetAssemblyFileName(assm);

				try
				{
					var diagnosticSink = CreateDiagnosticSink(assemblyFileName);
					var hasDiagnostics = diagnosticSink is not null;

					TestContext.SetForInitialization(
					diagnosticMessageSink: diagnosticSink,
					diagnosticMessages: hasDiagnostics,
					internalDiagnosticMessages: hasDiagnostics);

					var configuration = GetConfiguration(Path.GetFileNameWithoutExtension(assemblyFileName));

					var testFramework = ExtensibilityPointFactory.GetTestFramework(assm);
					await using var frameworkDisposal = testFramework as IAsyncDisposable;

					var frameworkDiscoverer = testFramework.GetDiscoverer(assm);

					var discoveredTestCases = new List<ITestCase>();

					var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);
					discoveryOptions.SetSynchronousMessageReporting(true);
					discoveryOptions.SetPreEnumerateTheories(true);

					await frameworkDiscoverer.Find(testCase =>
					{
						discoveredTestCases.Add(testCase);
						return new ValueTask<bool>(true);
					}, discoveryOptions, cancellationToken: cancellationToken);

					var testAssembly = new Xunit3TestAssemblyInfo(assemblyFileName, configuration);
					var testCases = discoveredTestCases
					.Select(tc => new Xunit3TestCaseInfo(
					testAssembly,
					tc.UniqueID,
					tc.TestCaseDisplayName,
					tc.TestClassName,
					tc.TestMethodName))
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

	Xunit3DiagnosticMessageSink? CreateDiagnosticSink(string assemblyFileName)
	{
		if (_diagnosticsManager is null)
			return null;

		return new Xunit3DiagnosticMessageSink(
		d => _diagnosticsManager.PostDiagnosticMessage(d),
		Path.GetFileNameWithoutExtension(assemblyFileName));
	}

	static TestAssemblyConfiguration GetConfiguration(string assemblyName)
	{
		var configuration = new TestAssemblyConfiguration();

		using var stream = GetConfigurationStreamForAssembly(assemblyName);
		if (stream is not null)
		{
			using var reader = new StreamReader(stream);
			var jsonText = reader.ReadToEnd();
			ConfigReader_Json.LoadFromJson(configuration, jsonText);
		}

		return configuration;
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
