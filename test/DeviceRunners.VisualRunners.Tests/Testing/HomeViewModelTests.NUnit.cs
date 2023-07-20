using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.NUnit;

namespace VisualRunnerTests.Testing;

public class NUnitHomeViewModelTests : HomeViewModelTests
{
	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new NUnitTestDiscoverer(configuration);

	public override ITestRunner CreateTestRunner() =>
		new NUnitTestRunner();
}
