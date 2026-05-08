using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

namespace VisualRunnerTests.Testing;

public class Xunit3HomeViewModelTests : HomeViewModelTests
{
public override Assembly TestAssembly => typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;

// xUnit v3 discovers 6 tests (output helper tests not discovered in class library mode)
public override int ExpectedTestCount => 6;

public override ITestDiscoverer CreateTestDiscoverer(VisualTestRunnerConfiguration configuration) =>
new Xunit3TestDiscoverer(configuration);

public override ITestRunner CreateTestRunner(VisualTestRunnerConfiguration configuration) =>
new Xunit3TestRunner(configuration);
}
