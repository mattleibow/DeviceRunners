using System.Reflection;

using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

public class Xunit3TestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<Assembly> _testAssemblies;

	public Xunit3TestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null)
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

			// Use logical name (not file path) so discovery works on
			// platforms without filesystem access such as WASM.
			var assemblyFileName = assm.GetName().Name + ".dll";

			try
			{
				// Run discovery on a thread pool thread to prevent xunit v3's
				// internal async infrastructure from capturing the caller's
				// SynchronizationContext (e.g. WinUI's DispatcherQueueSynchronizationContext).
				var assemblyInfo = await Task.Run(async () =>
				{
					var diagnosticSink = Xunit3DiagnosticMessageSink.TryCreate(_diagnosticsManager, assemblyFileName);
					var hasDiagnostics = diagnosticSink is not null;

					TestContext.SetForInitialization(
						diagnosticMessageSink: diagnosticSink,
						diagnosticMessages: hasDiagnostics,
						internalDiagnosticMessages: hasDiagnostics);

					var configuration = GetConfiguration(Path.GetFileNameWithoutExtension(assemblyFileName));

					var testFramework = CreateTestFramework(assm);
					await using var frameworkDisposal = testFramework as IAsyncDisposable;

					var frameworkDiscoverer = testFramework.GetDiscoverer(assm);
					await using var discovererDisposal = frameworkDiscoverer as IAsyncDisposable;

					var discoveredTestCases = new List<ITestCase>();

					var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);

					await frameworkDiscoverer.Find(testCase =>
					{
						discoveredTestCases.Add(testCase);
						return new ValueTask<bool>(!cancellationToken.IsCancellationRequested);
					}, discoveryOptions, cancellationToken: cancellationToken);

					var testAssembly = new Xunit3TestAssemblyInfo(assemblyFileName, configuration);
					var testCases = discoveredTestCases
						.Select(tc => new Xunit3TestCaseInfo(testAssembly, tc))
						.ToList();

					if (testCases.Count > 0)
					{
						testAssembly.TestCases.AddRange(testCases);
						return testAssembly;
					}

					return null;
				}, cancellationToken);

				if (assemblyInfo is not null)
					result.Add(assemblyInfo);
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

	static ITestFramework CreateTestFramework(Assembly assembly) =>
		InMemoryXunit3TestFramework.CreateForAssembly(assembly);
}
