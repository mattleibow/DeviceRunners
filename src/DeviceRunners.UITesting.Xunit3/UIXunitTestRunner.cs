using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Custom xUnit v3 test runner that dispatches only the test method invocation
/// to the UI thread. This matches the v2 behavior where class construction,
/// <see cref="IAsyncLifetime"/>, and disposal all run off the UI thread.
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
	protected override ValueTask<TimeSpan> InvokeTest(
		XunitTestRunnerContext ctxt,
		object? testClassInstance)
	{
		var task = UIThreadCoordinator.DispatchAsync(
			() => base.InvokeTest(ctxt, testClassInstance).AsTask());

		return new ValueTask<TimeSpan>(task);
	}
}
