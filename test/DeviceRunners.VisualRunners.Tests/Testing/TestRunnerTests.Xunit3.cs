using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

namespace VisualRunnerTests.Testing;

public class Xunit3TestRunnerTests : TestRunnerTests
{
	public override Assembly TestAssembly => typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;

	public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
		new Xunit3TestDiscoverer(configuration);

	public override ITestRunner CreateTestRunner(VisualTestRunnerConfiguration configuration) =>
		new Xunit3TestRunner(configuration);
}
