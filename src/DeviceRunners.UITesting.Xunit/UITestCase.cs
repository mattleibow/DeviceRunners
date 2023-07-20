using System.ComponentModel;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.UITesting.Xunit;

public class UITestCase : XunitTestCase
{
	public UITestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, object?[]? testMethodArguments = null)
		: base(diagnosticMessageSink, defaultMethodDisplay, TestMethodDisplayOptions.None, testMethod, testMethodArguments)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Called by the deserializer", true)]
	public UITestCase()
	{
	}

	public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) =>
		new UITestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();
}
