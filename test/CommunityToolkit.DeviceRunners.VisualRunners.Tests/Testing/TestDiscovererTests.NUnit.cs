using CommunityToolkit.DeviceRunners;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

namespace VisualRunnerTests.Testing;

public class NUnitTestDiscovererTests : TestDiscovererTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new NUnitTestDiscoverer(configuration);
}
