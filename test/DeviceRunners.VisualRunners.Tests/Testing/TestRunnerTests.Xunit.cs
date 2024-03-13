using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit;

namespace VisualRunnerTests.Testing;

public class XunitTestRunnerTests : TestRunnerTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new XunitTestDiscoverer(configuration);

	public override ITestRunner CreateTestRunner(VisualTestRunnerConfiguration configuration) =>
		new XunitTestRunner(configuration);
}
