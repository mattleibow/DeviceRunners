using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.NUnit;

namespace VisualRunnerTests.Testing;

public class NUnitTestDiscovererTests : TestDiscovererTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new NUnitTestDiscoverer(configuration);
}
