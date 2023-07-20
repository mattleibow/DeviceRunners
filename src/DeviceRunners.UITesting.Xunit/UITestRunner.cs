using System.Reflection;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.UITesting.Xunit;

public class UITestRunner : XunitTestRunner
{
	public UITestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		: base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
	{
	}

	protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
	{
		// var invoker = new UITestInvoker(Test, MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource);
		// invoker.RunAsync();

		var task = UIThreadCoordinator.DispatchAsync(() => base.InvokeTestMethodAsync(aggregator));

		// block the xUnit thread to ensure its concurrency throttle is effective
		var runSummary = task.GetAwaiter().GetResult();

		return Task.FromResult(runSummary);
	}
}
