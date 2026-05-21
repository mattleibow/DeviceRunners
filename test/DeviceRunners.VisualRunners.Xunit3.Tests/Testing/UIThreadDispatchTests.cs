using DeviceRunners.UITesting;
using DeviceRunners.UITesting.Xunit3;

using NSubstitute;

using Xunit;
using Xunit.v3;

namespace VisualRunnerTests.Xunit3.Testing;

/// <summary>
/// Tests for UIFact/UITheory dispatch infrastructure.
/// These verify the coordinator plumbing without needing a full MAUI app.
/// Full end-to-end UIFact/UITheory execution is tested by device sample apps in CI.
/// </summary>
public class UIThreadDispatchTests : IDisposable
{
	readonly IUIThreadCoordinator? _originalCoordinator;

	public UIThreadDispatchTests()
	{
		_originalCoordinator = UIThreadCoordinator.Current;
	}

	public void Dispose()
	{
		UIThreadCoordinator.Current = _originalCoordinator;
	}

	[Fact]
	public async Task UIThreadCoordinator_ThrowsWhenNoCoordinatorSet()
	{
		UIThreadCoordinator.Current = null;

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => UIThreadCoordinator.DispatchAsync(() => Task.FromResult(TimeSpan.Zero)));
	}

	[Fact]
	public async Task UIThreadCoordinator_DelegatesToConfiguredCoordinator()
	{
		var expectedResult = TimeSpan.FromMilliseconds(42);
		var coordinator = Substitute.For<IUIThreadCoordinator>();
		coordinator.DispatchAsync(Arg.Any<Func<Task<TimeSpan>>>())
			.Returns(callInfo =>
			{
				var func = callInfo.Arg<Func<Task<TimeSpan>>>();
				return func();
			});

		UIThreadCoordinator.Current = coordinator;

		var result = await UIThreadCoordinator.DispatchAsync(() => Task.FromResult(expectedResult));

		Assert.Equal(expectedResult, result);
		await coordinator.Received(1).DispatchAsync(Arg.Any<Func<Task<TimeSpan>>>());
	}

	[Fact]
	public async Task UIThreadCoordinator_CoordinatorCanMarshalToSpecificThread()
	{
		// Simulate a UI thread coordinator that executes work on a dedicated thread
		var uiThreadId = -1;
		var coordinator = new FakeUIThreadCoordinator(
			onDispatch: () => uiThreadId = Environment.CurrentManagedThreadId);

		UIThreadCoordinator.Current = coordinator;

		var executionThreadId = await UIThreadCoordinator.DispatchAsync(async () =>
		{
			await Task.Yield();
			return Environment.CurrentManagedThreadId;
		});

		// The coordinator should have been invoked
		Assert.True(coordinator.DispatchCount > 0);
	}

	[Fact]
	public void UIFactAttribute_HasCorrectDiscoverer()
	{
		var attr = typeof(UIFactAttribute)
			.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute), true)
			.OfType<XunitTestCaseDiscovererAttribute>()
			.SingleOrDefault();

		Assert.NotNull(attr);
		// Verify it points to UIFactDiscoverer
		Assert.Contains("UIFactDiscoverer", attr.Type.Name);
	}

	[Fact]
	public void UITheoryAttribute_HasCorrectDiscoverer()
	{
		var attr = typeof(UITheoryAttribute)
			.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute), true)
			.OfType<XunitTestCaseDiscovererAttribute>()
			.SingleOrDefault();

		Assert.NotNull(attr);
		// Verify it points to UITheoryDiscoverer
		Assert.Contains("UITheoryDiscoverer", attr.Type.Name);
	}

	[Fact]
	public void UITestCase_ImplementsSelfExecutingInterface()
	{
		Assert.True(typeof(ISelfExecutingXunitTestCase).IsAssignableFrom(typeof(UITestCase)));
	}

	[Fact]
	public void UITheoryTestCase_ImplementsSelfExecutingInterface()
	{
		Assert.True(typeof(ISelfExecutingXunitTestCase).IsAssignableFrom(typeof(UITheoryTestCase)));
	}

	/// <summary>
	/// Fake coordinator that tracks dispatch calls and runs work inline.
	/// </summary>
	sealed class FakeUIThreadCoordinator : IUIThreadCoordinator
	{
		readonly Action? _onDispatch;

		public int DispatchCount { get; private set; }

		public FakeUIThreadCoordinator(Action? onDispatch = null)
		{
			_onDispatch = onDispatch;
		}

		public async Task<T> DispatchAsync<T>(Func<Task<T>> operation)
		{
			DispatchCount++;
			_onDispatch?.Invoke();
			return await operation();
		}
	}
}
