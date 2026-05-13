using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Extends <see cref="XunitTestAssemblyRunner"/> to add cooperative yielding
/// between test collections for single-threaded WASM environments.
/// </summary>
class WasmXunitAssemblyRunner : XunitTestAssemblyRunner
{
	public WasmXunitAssemblyRunner(
		ITestAssembly testAssembly,
		IEnumerable<IXunitTestCase> testCases,
		IMessageSink diagnosticMessageSink,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions)
		: base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
	{
	}

	protected override async Task<RunSummary> RunTestCollectionAsync(
		IMessageBus messageBus,
		ITestCollection testCollection,
		IEnumerable<IXunitTestCase> testCases,
		CancellationTokenSource cancellationTokenSource)
	{
		await Task.Yield();

		var runner = new WasmXunitCollectionRunner(
			testCollection,
			testCases,
			DiagnosticMessageSink,
			messageBus,
			TestCaseOrderer,
			new ExceptionAggregator(Aggregator),
			cancellationTokenSource);
		return await runner.RunAsync();
	}
}
