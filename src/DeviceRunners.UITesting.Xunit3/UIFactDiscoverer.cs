using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom discoverer for <see cref="UIFactAttribute"/> that creates test cases
/// which execute test methods on the UI thread.
/// </summary>
public class UIFactDiscoverer : IXunitTestCaseDiscoverer
{
	public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

		return new(
		[
			new UITestCase(
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				details.SkipExceptions,
				details.SkipReason,
				details.SkipType,
				details.SkipUnless,
				details.SkipWhen,
				sourceFilePath: details.SourceFilePath,
				sourceLineNumber: details.SourceLineNumber,
				timeout: details.Timeout)
		]);
	}
}
