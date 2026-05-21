using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// A test case that executes a single [UIFact] test on the UI thread.
/// Implements <see cref="ISelfExecutingXunitTestCase"/> to hook into the xUnit v3
/// execution pipeline and dispatch the entire test lifecycle — class construction,
/// <see cref="IAsyncLifetime"/>, test method invocation, and disposal — to the UI thread.
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
		// Run CreateTests() through the aggregator so any failures are reported
		// as proper test results rather than unhandled exceptions.
		var tests = await aggregator.RunAsync(CreateTests, []);

		if (aggregator.ToException() is Exception ex)
		{
			if (ex.Message?.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal) == true)
			{
				return XunitRunnerHelper.SkipTestCases(
					messageBus,
					cancellationTokenSource,
					[this],
					ex.Message.Substring(DynamicSkipToken.Value.Length),
					sendTestCaseMessages: false);
			}
			else if (SkipExceptions?.Contains(ex.GetType()) == true)
			{
				return XunitRunnerHelper.SkipTestCases(
					messageBus,
					cancellationTokenSource,
					[this],
					ex.Message ?? $"Exception of type '{ex.GetType().FullName}' was thrown",
					sendTestCaseMessages: false);
			}
			else
			{
				return XunitRunnerHelper.FailTestCases(
					messageBus,
					cancellationTokenSource,
					[this],
					ex,
					sendTestCaseMessages: false);
			}
		}

		// Use UIXunitTestCaseRunner which dispatches the entire test lifecycle
		// (construction, IAsyncLifetime, test method, disposal) to the UI thread.
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
