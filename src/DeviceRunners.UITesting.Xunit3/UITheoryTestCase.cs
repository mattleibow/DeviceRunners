using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// A test case for [UITheory] test methods that executes on the UI thread.
/// Theory test cases with un-enumerable data get one test case for the entire theory.
/// </summary>
public class UITheoryTestCase : XunitDelayEnumeratedTheoryTestCase, ISelfExecutingXunitTestCase
{
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public UITheoryTestCase()
	{
	}

	public UITheoryTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		bool skipTestWithoutData,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null)
		: base(
			testMethod,
			testCaseDisplayName,
			uniqueID,
			@explicit,
			skipTestWithoutData,
			skipExceptions,
			skipReason,
			skipType,
			skipUnless,
			skipWhen,
			traits,
			sourceFilePath,
			sourceLineNumber,
			timeout)
	{
	}

	public async ValueTask<RunSummary> Run(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		return await UIThreadCoordinator.DispatchAsync(async () =>
		{
			var tests = await CreateTests();
			return await XunitTestCaseRunner.Instance.Run(
				this,
				tests,
				messageBus,
				aggregator,
				cancellationTokenSource,
				TestCaseDisplayName,
				SkipReason,
				explicitOption,
				constructorArguments);
		});
	}
}
