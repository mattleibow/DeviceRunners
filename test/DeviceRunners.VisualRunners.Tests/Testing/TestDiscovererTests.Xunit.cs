using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitTestDiscovererTests : TestDiscovererTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitTestDiscoverer(configuration);
}
