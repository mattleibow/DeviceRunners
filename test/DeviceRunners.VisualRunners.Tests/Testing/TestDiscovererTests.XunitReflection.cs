using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitReflectionTestDiscovererTests : TestDiscovererTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitReflectionTestDiscoverer(configuration);
}
