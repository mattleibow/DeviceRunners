using System.Reflection;

using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

public class Xunit3TestRunner : ITestRunner
{
	readonly AsyncLock _executionLock = new();

	readonly IVisualTestRunnerConfiguration _options;
	readonly IResultChannelManager? _resultChannelManager;
	readonly IDiagnosticsManager? _diagnosticsManager;

	public Xunit3TestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
	{
		_options = options;
		_resultChannelManager = resultChannelManager;
		_diagnosticsManager = diagnosticsManager;
	}

	public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
	{
		var grouped = testCases
		.OfType<Xunit3TestCaseInfo>()
		.GroupBy(t => t.TestAssembly)
		.Select(g => new Xunit3TestAssemblyInfo(g.Key, g.ToList()))
		.ToList();

		return RunTestsAsync(grouped, cancellationToken);
	}

	public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
	{
		using (await _executionLock.LockAsync())
		{
			await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

			var xunit3Assemblies = testAssemblies.OfType<Xunit3TestAssemblyInfo>().ToList();
			if (xunit3Assemblies.Count == 0)
				return;

			foreach (var assembly in xunit3Assemblies)
			{
				await RunTests(assembly, cancellationToken);
			}
		}
	}

	async Task RunTests(Xunit3TestAssemblyInfo assemblyInfo, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		var assemblyFileName = assemblyInfo.AssemblyFileName;

		// Match by logical name so it works on platforms where
		// Assembly.Location returns empty string (Android, iOS, WASM).
		var assembly = _options.TestAssemblies
			.FirstOrDefault(a => string.Equals(
				a.GetName().Name + ".dll",
				assemblyFileName,
				StringComparison.OrdinalIgnoreCase));

		if (assembly is null)
			return;

		var testCaseLookup = assemblyInfo.TestCases
			.ToDictionary(tc => tc.TestCaseUniqueID, tc => tc);

		// Use cached ITestCase objects from discovery — no re-discovery needed.
		var testCasesToRun = assemblyInfo.TestCases
			.Select(tc => tc.TestCase)
			.ToList();

		if (testCasesToRun.Count == 0)
			return;

		Xunit3DiagnosticMessageSink? diagnosticSink = null;
		if (_diagnosticsManager is not null)
		{
			diagnosticSink = new Xunit3DiagnosticMessageSink(
				d => _diagnosticsManager.PostDiagnosticMessage(d),
				Path.GetFileNameWithoutExtension(assemblyFileName));
		}

		var hasDiagnostics = diagnosticSink is not null;
		TestContext.SetForInitialization(
			diagnosticMessageSink: diagnosticSink,
			diagnosticMessages: hasDiagnostics,
			internalDiagnosticMessages: hasDiagnostics);

		var testFramework = CreateTestFramework(assembly);
		await using var frameworkDisposal = testFramework as IAsyncDisposable;

		var configuration = assemblyInfo.Configuration;
		var executor = testFramework.GetExecutor(assembly);

		var executionOptions = TestFrameworkOptions.ForExecution(configuration);
		executionOptions.SetSynchronousMessageReporting(true);

		var resultSink = new Xunit3ExecutionMessageSink(testCaseLookup, _resultChannelManager, _diagnosticsManager, cancellationToken);

		await executor.RunTestCases(testCasesToRun, resultSink, executionOptions, cancellationToken);
	}

	static ITestFramework CreateTestFramework(Assembly assembly) =>
		InMemoryXunit3TestFramework.CreateForAssembly(assembly);
}
