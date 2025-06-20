using System.Reflection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

public class ThreadlessXunitTestDiscoverer : ITestDiscoverer
{
	readonly IDiagnosticsManager? _diagnosticsManager;
	readonly IReadOnlyList<Assembly> _testAssemblies;

	public ThreadlessXunitTestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<ThreadlessXunitTestDiscoverer>? logger = null)
	{
		_diagnosticsManager = diagnosticsManager;
		_testAssemblies = options.TestAssemblies.ToArray();
	}

	public Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(Discover(cancellationToken));

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
					var assemblyInfo = new ReflectionAssemblyInfo(assm);
					var sink = new TestDiscoverySink();
					var discoverer = new ThreadlessXunitFrameworkDiscoverer(assemblyInfo, new NullSourceInformationProvider(), sink);
					discoverer.FindWithoutThreads(false, sink, discoveryOptions);
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

		return new TestAssemblyConfiguration
		{
			ShadowCopy = false,
			ParallelizeAssembly = false,
			ParallelizeTestCollections = false,
			MaxParallelThreads = 1,
			PreEnumerateTheories = false
		};
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

internal class ThreadlessXunitFrameworkDiscoverer : XunitTestFrameworkDiscoverer
{
	public ThreadlessXunitFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink)
		: base(assemblyInfo, sourceProvider, diagnosticMessageSink)
	{
	}

	public void FindWithoutThreads(bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		using var messageBus = new SynchronousMessageBus(discoveryMessageSink, false);

		foreach (var type in AssemblyInfo.GetTypes(includePrivateTypes: false).Where(IsValidTestClass))
		{
			var testClass = CreateTestClass(type);
			if (!FindTestsForType(testClass, includeSourceInformation, messageBus, discoveryOptions))
				break;
		}

		messageBus.QueueMessage(new global::Xunit.Sdk.DiscoveryCompleteMessage());
	}
}