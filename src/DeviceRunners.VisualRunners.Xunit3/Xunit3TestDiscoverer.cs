using System.Reflection;

using Microsoft.Extensions.Logging;

using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

public class Xunit3TestDiscoverer : ITestDiscoverer
{
readonly IDiagnosticsManager? _diagnosticsManager;
readonly IReadOnlyList<Assembly> _testAssemblies;

public Xunit3TestDiscoverer(IVisualTestRunnerConfiguration options, IDiagnosticsManager? diagnosticsManager = null, ILogger<Xunit3TestDiscoverer>? logger = null)
{
_diagnosticsManager = diagnosticsManager;
_testAssemblies = options.TestAssemblies.ToArray();
}

public Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default) =>
AsyncUtils.RunAsync(() => Discover(cancellationToken));

IReadOnlyList<ITestAssemblyInfo> Discover(CancellationToken cancellationToken = default)
{
var result = new List<ITestAssemblyInfo>();

try
{
foreach (var assm in _testAssemblies)
{
if (cancellationToken.IsCancellationRequested)
break;

var assemblyFileName = FileSystemUtils.GetAssemblyFileName(assm);

try
{
var testFramework = ExtensibilityPointFactory.GetTestFramework(assm);
var frameworkDiscoverer = testFramework.GetDiscoverer(assm);

var discoveredTestCases = new List<ITestCase>();

var discoveryOptions = TestFrameworkOptions.ForDiscovery(new TestAssemblyConfiguration());
discoveryOptions.SetSynchronousMessageReporting(true);

frameworkDiscoverer.Find(testCase =>
{
discoveredTestCases.Add(testCase);
return new ValueTask<bool>(true);
}, discoveryOptions);

var testAssembly = new Xunit3TestAssemblyInfo(assemblyFileName);
var testCases = discoveredTestCases
.Select(tc => new Xunit3TestCaseInfo(
testAssembly,
tc.UniqueID,
tc.TestCaseDisplayName,
tc.TestClassName,
tc.TestMethodName))
.ToList();

if (testCases.Count > 0)
{
testAssembly.TestCases.AddRange(testCases);
result.Add(testAssembly);
}
}
catch (Exception ex)
{
_diagnosticsManager?.PostDiagnosticMessage($"Exception discovering tests in assembly '{assemblyFileName}': '{ex.Message}'{Environment.NewLine}{ex}");
}
}
}
catch (Exception ex)
{
_diagnosticsManager?.PostDiagnosticMessage($"Exception discovering tests: '{ex.Message}'{Environment.NewLine}{ex}");
}

return result;
}
}
