using CommunityToolkit.DeviceRunners;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitTestDiscovererTests : TestDiscovererTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitTestDiscoverer(configuration);
}
