using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

#pragma warning disable CS0618 // SynchronousMessageBus is obsolete but required for threadless WASM operation

/// <summary>
/// Extends <see cref="XunitTestFrameworkDiscoverer"/> to perform test discovery
/// without spawning threads, which is required for single-threaded WASM environments.
/// Uses <see cref="SynchronousMessageBus"/> to process discovery messages inline.
/// </summary>
class WasmXunitDiscoverer : XunitTestFrameworkDiscoverer
{
	public WasmXunitDiscoverer(
		IAssemblyInfo assemblyInfo,
		ISourceInformationProvider sourceProvider,
		IMessageSink diagnosticMessageSink)
		: base(assemblyInfo, sourceProvider, diagnosticMessageSink)
	{
	}

	/// <summary>
	/// Discovers all tests in the assembly without using threads.
	/// Handles all xunit test types: Fact, Theory, MemberData, ClassData, etc.
	/// </summary>
	public List<ITestCase> DiscoverTests(ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		var sink = new TestCaseCollector();

		using var messageBus = new SynchronousMessageBus(sink);
		foreach (var type in AssemblyInfo.GetTypes(includePrivateTypes: false).Where(IsValidTestClass))
		{
			var testClass = CreateTestClass(type);
			if (!FindTestsForType(testClass, includeSourceInformation: false, messageBus, discoveryOptions))
				break;
		}

		return sink.TestCases;
	}

	class TestCaseCollector : LongLivedMarshalByRefObject, IMessageSink
	{
		public List<ITestCase> TestCases { get; } = [];

		public bool OnMessage(IMessageSinkMessage message)
		{
			if (message is ITestCaseDiscoveryMessage discovery)
				TestCases.Add(discovery.TestCase);
			return true;
		}
	}
}

#pragma warning restore CS0618
