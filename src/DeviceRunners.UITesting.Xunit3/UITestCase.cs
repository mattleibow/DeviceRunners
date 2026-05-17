using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// A test case that executes a single [UIFact] test method on the UI thread.
/// Implements <see cref="ISelfExecutingXunitTestCase"/> to hook into the xUnit v3
/// execution pipeline and dispatch only the test method invocation to the UI thread,
/// while class construction, <see cref="IAsyncLifetime"/>, and disposal run on the
/// xUnit worker thread.
/// </summary>
public class UITestCase : XunitTestCase, ISelfExecutingXunitTestCase
{
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public UITestCase()
	{
	}

	public UITestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		object?[]? testMethodArguments = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null)
		: base(
			testMethod,
			testCaseDisplayName,
			uniqueID,
			@explicit,
			skipExceptions,
			skipReason,
			skipType,
			skipUnless,
			skipWhen,
			traits,
			testMethodArguments,
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
