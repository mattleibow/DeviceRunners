using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// A test case for [UITheory] test methods that executes the test method on the UI thread.
/// Theory test cases with un-enumerable data get one test case for the entire theory.
/// Only the test method invocation is dispatched to the UI thread; class construction,
/// <see cref="IAsyncLifetime"/>, and disposal run on the xUnit worker thread.
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
		var tests = await CreateTests();

		// Use UIXunitTestCaseRunner which dispatches only InvokeTest to the UI thread,
		// keeping construction, IAsyncLifetime, and disposal on the worker thread.
		return await UIXunitTestCaseRunner.Instance.Run(
			this,
			tests,
			messageBus,
			aggregator,
			cancellationTokenSource,
			TestCaseDisplayName,
			SkipReason,
			explicitOption,
			constructorArguments);
	}
}
