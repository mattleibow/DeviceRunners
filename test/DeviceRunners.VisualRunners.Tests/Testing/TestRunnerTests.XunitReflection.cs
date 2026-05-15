using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitReflectionTestRunnerTests : TestRunnerTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitReflectionTestDiscoverer(configuration);

	public override ITestRunner CreateTestRunner(VisualTestRunnerConfiguration configuration) =>
		new XunitReflectionTestRunner(configuration);
}
