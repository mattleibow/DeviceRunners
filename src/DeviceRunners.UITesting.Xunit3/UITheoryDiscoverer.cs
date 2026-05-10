using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom discoverer for <see cref="UITheoryAttribute"/> that creates test cases
/// which execute test methods on the UI thread.
/// </summary>
public class UITheoryDiscoverer : IXunitTestCaseDiscoverer
{
	public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);
		var skipTestWithoutData = (factAttribute as ITheoryAttribute)?.SkipTestWithoutData ?? false;

		return new(
		[
			new UITheoryTestCase(
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				skipTestWithoutData,
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
