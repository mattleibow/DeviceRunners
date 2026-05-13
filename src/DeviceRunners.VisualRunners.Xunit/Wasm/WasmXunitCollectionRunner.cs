using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Extends <see cref="XunitTestCollectionRunner"/> to add cooperative yielding
/// between test classes for single-threaded WASM environments.
/// </summary>
class WasmXunitCollectionRunner : XunitTestCollectionRunner
{
	public WasmXunitCollectionRunner(
		ITestCollection testCollection,
		IEnumerable<IXunitTestCase> testCases,
		IMessageSink diagnosticMessageSink,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
		: base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
	{
	}

	protected override async Task<RunSummary> RunTestClassAsync(
		ITestClass testClass,
		IReflectionTypeInfo @class,
		IEnumerable<IXunitTestCase> testCases)
	{
		await Task.Yield();

		var runner = new WasmXunitClassRunner(
			testClass,
			@class,
			testCases,
			DiagnosticMessageSink,
			MessageBus,
			TestCaseOrderer,
			new ExceptionAggregator(Aggregator),
			CancellationTokenSource,
			CollectionFixtureMappings);
		return await runner.RunAsync();
	}
}
