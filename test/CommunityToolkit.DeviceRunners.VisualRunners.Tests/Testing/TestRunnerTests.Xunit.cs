using CommunityToolkit.DeviceRunners;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitTestRunnerTests : TestRunnerTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitTestDiscoverer(configuration);

	public override ITestRunner CreateTestRunner() =>
		new XunitTestRunner();
}
