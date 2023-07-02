using System.Reflection;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest;

public class UITestInvoker : XunitTestInvoker
{
	public UITestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		: base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
	{
	}

	protected override Task<decimal> InvokeTestMethodAsync(object testClassInstance)
	{
		var task = Application.Current.Dispatcher.DispatchAsync(() =>
			base.InvokeTestMethodAsync(testClassInstance));

		// block the xUnit thread to ensure its concurrency throttle is effective
		var runSummary = task.GetAwaiter().GetResult();

		return Task.FromResult(runSummary);
	}
}
