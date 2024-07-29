using Xunit.Sdk;

namespace DeviceRunners.UITesting.Xunit;

public class UITestCaseRunner : XunitTestCaseRunner
{
	public UITestCaseRunner(IXunitTestCase testCase, string displayName, string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		: base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
	{
	}

	protected override Task<RunSummary> RunTestAsync() =>
		new UITestRunner(new XunitTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes, Aggregator, CancellationTokenSource).RunAsync();
}
