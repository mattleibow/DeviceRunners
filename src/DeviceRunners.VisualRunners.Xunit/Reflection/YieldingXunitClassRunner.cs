using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Extends <see cref="XunitTestClassRunner"/> to add cooperative yielding
/// between test methods for single-threaded environments.
/// </summary>
class YieldingXunitClassRunner : XunitTestClassRunner
{
	public YieldingXunitClassRunner(
		ITestClass testClass,
		IReflectionTypeInfo @class,
		IEnumerable<IXunitTestCase> testCases,
		IMessageSink diagnosticMessageSink,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IDictionary<Type, object> collectionFixtureMappings)
		: base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
	{
	}

	protected override async Task<RunSummary> RunTestMethodAsync(
		ITestMethod testMethod,
		IReflectionMethodInfo method,
		IEnumerable<IXunitTestCase> testCases,
		object[] constructorArguments)
	{
		await Task.Yield();
		return await base.RunTestMethodAsync(testMethod, method, testCases, constructorArguments);
	}
}
