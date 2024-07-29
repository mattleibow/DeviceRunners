using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.UITesting.Xunit;

public class UIFactDiscoverer : FactDiscoverer
{
	public UIFactDiscoverer(IMessageSink diagnosticMessageSink)
		: base(diagnosticMessageSink)
	{
	}

	protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute) =>
		new UITestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
}
