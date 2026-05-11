using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom discoverer for <see cref="UITheoryAttribute"/> that creates test cases
/// which execute test methods on the UI thread.
/// Derives from xUnit v3's <see cref="TheoryDiscoverer"/> to inherit all theory
/// enumeration, serialization, trait handling, and no-data error handling.
/// </summary>
public class UITheoryDiscoverer : TheoryDiscoverer
{
protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
ITestFrameworkDiscoveryOptions discoveryOptions,
IXunitTestMethod testMethod,
ITheoryAttribute theoryAttribute,
ITheoryDataRow dataRow,
object?[] testMethodArguments)
{
var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments);
var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

var testCase = new UITestCase(
details.ResolvedTestMethod,
details.TestCaseDisplayName,
details.UniqueID,
details.Explicit,
details.SkipExceptions,
details.SkipReason,
details.SkipType,
details.SkipUnless,
details.SkipWhen,
traits,
testMethodArguments,
sourceFilePath: details.SourceFilePath,
sourceLineNumber: details.SourceLineNumber,
timeout: details.Timeout);

return new(new[] { (IXunitTestCase)testCase });
}

protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
ITestFrameworkDiscoveryOptions discoveryOptions,
IXunitTestMethod testMethod,
ITheoryAttribute theoryAttribute)
{
var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
var testCase =
details.SkipReason is not null && details.SkipUnless is null && details.SkipWhen is null
? (IXunitTestCase)new UITestCase(
details.ResolvedTestMethod,
details.TestCaseDisplayName,
details.UniqueID,
details.Explicit,
details.SkipExceptions,
details.SkipReason,
details.SkipType,
details.SkipUnless,
details.SkipWhen,
TraitsHelper.ToReadWrite(testMethod.Traits),
sourceFilePath: details.SourceFilePath,
sourceLineNumber: details.SourceLineNumber,
timeout: details.Timeout)
: new UITheoryTestCase(
details.ResolvedTestMethod,
details.TestCaseDisplayName,
details.UniqueID,
details.Explicit,
theoryAttribute.SkipTestWithoutData,
details.SkipExceptions,
details.SkipReason,
details.SkipType,
details.SkipUnless,
details.SkipWhen,
TraitsHelper.ToReadWrite(testMethod.Traits),
sourceFilePath: details.SourceFilePath,
sourceLineNumber: details.SourceLineNumber,
timeout: details.Timeout);

return new(new[] { testCase });
}
}
