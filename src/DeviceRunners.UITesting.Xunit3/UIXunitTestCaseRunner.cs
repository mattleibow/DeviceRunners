using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom xUnit v3 test case runner that uses <see cref="UIXunitTestRunner"/>
/// to dispatch test method invocation to the UI thread.
/// </summary>
public class UIXunitTestCaseRunner : XunitTestCaseRunner
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="UIXunitTestCaseRunner"/>.
	/// </summary>
	public static new UIXunitTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="UIXunitTestCaseRunner"/> class.
	/// </summary>
	protected UIXunitTestCaseRunner()
	{
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTest(
		XunitTestCaseRunnerContext ctxt,
		IXunitTest test)
	{
		return UIXunitTestRunner.Instance.Run(
			test,
			ctxt.MessageBus,
			ctxt.ConstructorArguments,
			ctxt.ExplicitOption,
			ctxt.Aggregator.Clone(),
			ctxt.CancellationTokenSource,
			ctxt.BeforeAfterTestAttributes);
	}
}
