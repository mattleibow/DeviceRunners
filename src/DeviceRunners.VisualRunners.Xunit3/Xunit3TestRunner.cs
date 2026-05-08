using System.Reflection;

using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

public class Xunit3TestRunner : ITestRunner
{
readonly AsyncLock _executionLock = new();

readonly IVisualTestRunnerConfiguration _options;
readonly IResultChannelManager? _resultChannelManager;
readonly IDiagnosticsManager? _diagnosticsManager;

public Xunit3TestRunner(IVisualTestRunnerConfiguration options, IResultChannelManager? resultChannelManager = null, IDiagnosticsManager? diagnosticsManager = null)
{
_options = options;
_resultChannelManager = resultChannelManager;
_diagnosticsManager = diagnosticsManager;
}

public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default)
{
var grouped = testCases
.OfType<Xunit3TestCaseInfo>()
.GroupBy(t => t.TestAssembly)
.Select(g => new Xunit3TestAssemblyInfo(g.Key, g.ToList()))
.ToList();

return RunTestsAsync(grouped, cancellationToken);
}

public async Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
{
using (await _executionLock.LockAsync())
{
await using var closing = await ResultChannelManagerScope.OpenAsync(_resultChannelManager);

await AsyncUtils.RunAsync(() => RunTests(testAssemblies, cancellationToken));
}
}

void RunTests(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default)
{
var xunit3Assemblies = testAssemblies.OfType<Xunit3TestAssemblyInfo>().ToList();
if (xunit3Assemblies.Count == 0)
return;

foreach (var assembly in xunit3Assemblies)
{
RunTests(assembly, cancellationToken);
}
}

void RunTests(Xunit3TestAssemblyInfo assemblyInfo, CancellationToken cancellationToken = default)
{
if (cancellationToken.IsCancellationRequested)
return;

var assemblyFileName = assemblyInfo.AssemblyFileName;

var assembly = _options.TestAssemblies
.FirstOrDefault(a => string.Equals(
FileSystemUtils.GetAssemblyFileName(a),
assemblyFileName,
StringComparison.OrdinalIgnoreCase));

if (assembly is null)
return;

var testCaseLookup = assemblyInfo.TestCases
.ToDictionary(tc => tc.TestCaseUniqueID, tc => tc);

var testCaseIdsToRun = new HashSet<string>(assemblyInfo.TestCases.Select(tc => tc.TestCaseUniqueID));

var testFramework = ExtensibilityPointFactory.GetTestFramework(assembly);

// Discover to get ITestCase objects, then run selected ones
var frameworkDiscoverer = testFramework.GetDiscoverer(assembly);
var discoveredTestCases = new List<ITestCase>();

var discoveryOptions = TestFrameworkOptions.ForDiscovery(new TestAssemblyConfiguration());
discoveryOptions.SetSynchronousMessageReporting(true);

frameworkDiscoverer.Find(testCase =>
{
if (testCaseIdsToRun.Contains(testCase.UniqueID))
discoveredTestCases.Add(testCase);
return new ValueTask<bool>(true);
}, discoveryOptions);

if (discoveredTestCases.Count == 0)
return;

var executor = testFramework.GetExecutor(assembly);

var executionOptions = TestFrameworkOptions.ForExecution(new TestAssemblyConfiguration());
executionOptions.SetSynchronousMessageReporting(true);

var resultSink = new Xunit3ExecutionMessageSink(testCaseLookup, _resultChannelManager, cancellationToken);

executor.RunTestCases(discoveredTestCases, resultSink, executionOptions, cancellationToken);

resultSink.Finished.Wait(cancellationToken);
}
}
