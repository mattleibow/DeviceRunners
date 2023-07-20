using System.ComponentModel;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.UITesting.Xunit;

public class UITheoryTestCase : XunitTheoryTestCase
{
	public UITheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
		: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public UITheoryTestCase()
	{
	}

	public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) =>
		new UITheoryTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource).RunAsync();
}
