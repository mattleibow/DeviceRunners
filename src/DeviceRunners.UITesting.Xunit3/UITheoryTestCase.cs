using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// A test case for [UITheory] test methods that executes on the UI thread.
/// Theory test cases with un-enumerable data get one test case for the entire theory.
/// The entire test lifecycle — class construction, <see cref="IAsyncLifetime"/>,
/// test method invocation, and disposal — is dispatched to the UI thread.
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
		// Run CreateTests() through the aggregator so data enumeration failures
		// (e.g., bad MemberData, empty ClassData, SkipTestWithoutData) are reported
		// as proper test failures/skips rather than unhandled exceptions.
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
