using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom xUnit v3 test runner that dispatches the entire test execution
/// (class construction, <see cref="IAsyncLifetime"/>, test method invocation,
/// and disposal) to the UI thread.
/// <para>
/// In xUnit v2 the single <c>InvokeTestMethodAsync</c> override encompassed
/// class creation, lifecycle, method invocation, and disposal, so dispatching
/// that one method was sufficient. In v3 the pipeline splits these into
/// <c>CreateTestClassInstance</c>, <c>InvokeTest</c>, and
/// <c>DisposeTestClassInstance</c>. To keep everything on the UI thread we
/// override <see cref="RunTest"/> instead, which orchestrates all of them.
/// </para>
/// </summary>
public class UIXunitTestRunner : XunitTestRunner
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="UIXunitTestRunner"/>.
	/// </summary>
	public static new UIXunitTestRunner Instance { get; } = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="UIXunitTestRunner"/> class.
	/// </summary>
	protected UIXunitTestRunner()
	{
	}

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> RunTest(XunitTestRunnerContext ctxt)
	{
		var task = UIThreadCoordinator.DispatchAsync(
			() => base.RunTest(ctxt).AsTask());

		return new ValueTask<TimeSpan>(task);
	}
}
