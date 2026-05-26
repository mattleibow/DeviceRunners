using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom discoverer for <see cref="UIFactAttribute"/> that creates <see cref="UITestCase"/>
/// instances which execute test methods on the UI thread.
/// Derives from xUnit v3's <see cref="FactDiscoverer"/> to inherit all validation
/// (parameter checks, generic method checks) and trait handling.
/// </summary>
public class UIFactDiscoverer : FactDiscoverer
{
	protected override IXunitTestCase CreateTestCase(
	ITestFrameworkDiscoveryOptions discoveryOptions,
	IXunitTestMethod testMethod,
	IFactAttribute factAttribute)
	{
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

		return new UITestCase(
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
		timeout: details.Timeout);
	}
}
