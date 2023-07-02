using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest;

public class UIFactDiscoverer : FactDiscoverer
{
	public UIFactDiscoverer(IMessageSink diagnosticMessageSink)
		: base(diagnosticMessageSink)
	{
	}

	protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute) =>
		new UITestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
}
