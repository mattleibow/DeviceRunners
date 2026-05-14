using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitReflectionTestDiscovererTests : TestDiscovererTests
{
	// PreEnumerateTheories = false means Theory with 3 InlineData counts as 1 test case
	protected override int ExpectedTestCount => Constants.TestCountNoTheoryEnumeration;

	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitReflectionTestDiscoverer(configuration);
}
