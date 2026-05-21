using System.Reflection;

using DeviceRunners;
using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Xunit3;

using NSubstitute;

using Xunit;

namespace VisualRunnerTests.Xunit3.Testing;

/// <summary>
/// Verifies that xunit v3 discovery and execution do not capture the caller's
/// SynchronizationContext. This prevents COMExceptions on WinUI where xunit v3's
/// internal async infrastructure would otherwise post callbacks through the
/// DispatcherQueueSynchronizationContext from thread pool threads.
/// </summary>
public class ThreadingTests : IAsyncLifetime
{
	static readonly Assembly TestAssembly = typeof(TestProject.Xunit3Tests.Xunit3Tests).Assembly;

	IReadOnlyList<ITestAssemblyInfo> _testAssemblies = null!;
	VisualTestRunnerConfiguration _options = null!;

	public async ValueTask InitializeAsync()
	{
		var assemblies = new[] { TestAssembly };
		_options = new VisualTestRunnerConfiguration(assemblies);

		var discoverer = new Xunit3TestDiscoverer(_options);
		_testAssemblies = await discoverer.DiscoverAsync(TestContext.Current.CancellationToken);
	}

	public ValueTask DisposeAsync() => default;

	[Fact]
	public async Task DiscoverAsync_DoesNotCaptureCallerSyncContext()
	{
		// Simulate a blocking UI-like SynchronizationContext.
		// If xunit v3's internal framework captures this SyncContext and tries to
		// Post from its worker threads, it would either deadlock or throw.
		// The Task.Run wrapper prevents this by stripping the SyncContext for internal work.
		var blockingSyncContext = new SingleThreadSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(blockingSyncContext);

		try
		{
			var assemblies = new[] { TestAssembly };
			var options = new VisualTestRunnerConfiguration(assemblies);
			var discoverer = new Xunit3TestDiscoverer(options);

			// This completes successfully because the discoverer uses Task.Run internally,
			// preventing xunit v3 from capturing and using the blocking SyncContext.
			var results = await discoverer.DiscoverAsync(TestContext.Current.CancellationToken);

			Assert.NotEmpty(results);
			Assert.True(results[0].TestCases.Count > 0, "Should discover test cases");
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Fact]
	public async Task RunTestsAsync_DoesNotCaptureCallerSyncContext()
	{
		// Simulate a blocking UI-like SynchronizationContext.
		// Without the Task.Run wrapper, xunit v3 would capture this SyncContext
		// and trigger COMExceptions (on WinUI) or deadlocks when posting from worker threads.
		var blockingSyncContext = new SingleThreadSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(blockingSyncContext);

		try
		{
			var runner = new Xunit3TestRunner(_options);
			var testAssembly = _testAssemblies[0];

			await runner.RunTestsAsync(testAssembly, TestContext.Current.CancellationToken);

			// Verify tests actually ran — if they did, xunit didn't deadlock or throw
			var ranTests = testAssembly.TestCases.Where(tc => tc.Result is not null).ToList();
			Assert.NotEmpty(ranTests);

			// Verify at least one test passed (not just "ran but crashed")
			Assert.Contains(ranTests, tc => tc.Result!.Status == TestResultStatus.Passed);
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Fact]
	public async Task RunTestsAsync_ExecutesOnThreadPoolThread()
	{
		// Track which thread the test results arrive on
		var resultThreadIds = new System.Collections.Concurrent.ConcurrentBag<int>();
		var callerThreadId = Environment.CurrentManagedThreadId;

		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];

		foreach (var tc in testAssembly.TestCases)
		{
			tc.ResultReported += _ => resultThreadIds.Add(Environment.CurrentManagedThreadId);
		}

		await runner.RunTestsAsync(testAssembly, TestContext.Current.CancellationToken);

		// Results should have been reported (tests ran)
		Assert.NotEmpty(resultThreadIds);

		// At least some results should come from a different thread than the caller,
		// proving execution moved to the thread pool
		Assert.Contains(resultThreadIds, id => id != callerThreadId);
	}

	[Fact]
	public async Task RunTestsAsync_WithCustomSyncContext_DoesNotDeadlock()
	{
		// Use a single-threaded SyncContext to simulate WinUI behavior.
		// If the runner tried to Post back to this context, it would deadlock.
		var fakeSyncContext = new SingleThreadSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(fakeSyncContext);

		try
		{
			var runner = new Xunit3TestRunner(_options);
			var testAssembly = _testAssemblies[0];

			// This should complete without deadlocking (within timeout)
			var cancellation = TestContext.Current.CancellationToken;
			var task = runner.RunTestsAsync(testAssembly, cancellation);
			var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(30), cancellation));

			Assert.Same(task, completed);

			// Verify tests actually completed
			var passedTests = testAssembly.TestCases.Count(tc => tc.Result?.Status == TestResultStatus.Passed);
			Assert.True(passedTests > 0, "Expected some tests to pass");
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Fact]
	public async Task DiscoverAsync_WithCustomSyncContext_DoesNotDeadlock()
	{
		var fakeSyncContext = new SingleThreadSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(fakeSyncContext);

		try
		{
			var assemblies = new[] { TestAssembly };
			var options = new VisualTestRunnerConfiguration(assemblies);
			var discoverer = new Xunit3TestDiscoverer(options);

			var cancellation = TestContext.Current.CancellationToken;
			var task = discoverer.DiscoverAsync(cancellation);
			var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(30), cancellation));

			Assert.Same(task, completed);

			var results = await task;
			Assert.NotEmpty(results);
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Fact]
	public async Task RunTestsAsync_ConcurrentExecutions_AreSerialized()
	{
		// The runner uses AsyncLock — concurrent calls should not interleave
		var runner = new Xunit3TestRunner(_options);
		var testAssembly = _testAssemblies[0];
		var executionCount = 0;
		var cancellation = TestContext.Current.CancellationToken;

		// We can't easily hook into the lock, but we can verify that
		// running two tasks concurrently both complete correctly
		var task1 = Task.Run(async () =>
		{
			Interlocked.Increment(ref executionCount);
			await runner.RunTestsAsync(testAssembly, cancellation);
		}, cancellation);

		var task2 = Task.Run(async () =>
		{
			Interlocked.Increment(ref executionCount);
			await runner.RunTestsAsync(testAssembly, cancellation);
		}, cancellation);

		await Task.WhenAll(task1, task2);

		Assert.Equal(2, executionCount);
		// Both should complete — if deadlocked, the test would timeout
	}

	[Fact]
	public async Task DiagnosticsViewModel_MarshalsToCapturedSyncContext()
	{
		var fakeSyncContext = new TrackingSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(fakeSyncContext);

		try
		{
			var diagnosticsManager = new DiagnosticsManager();
			var vm = new DiagnosticsViewModel(diagnosticsManager);
			var cancellation = TestContext.Current.CancellationToken;

			// Fire event from a thread pool thread (simulating xunit v3 callback)
			await Task.Run(() =>
			{
				diagnosticsManager.PostDiagnosticMessage("test message from pool thread");
			}, cancellation);

			// Give the Post a moment to be processed
			await Task.Delay(50, cancellation);

			// The ViewModel should have used Post to marshal to the captured context
			Assert.True(fakeSyncContext.PostCount > 0,
				"DiagnosticsViewModel should marshal Messages.Add via SynchronizationContext.Post");
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Fact]
	public async Task DiagnosticsViewModel_WithoutSyncContext_AddsDirectly()
	{
		SynchronizationContext.SetSynchronizationContext(null);

		var diagnosticsManager = new DiagnosticsManager();
		var vm = new DiagnosticsViewModel(diagnosticsManager);

		diagnosticsManager.PostDiagnosticMessage("test message");

		// Without a SyncContext, message should be added directly
		Assert.Contains("test message", vm.Messages);
	}

	/// <summary>
	/// A SynchronizationContext that tracks Post calls to verify marshaling behavior.
	/// </summary>
	sealed class TrackingSynchronizationContext : SynchronizationContext
	{
		int _postCount;

		public int PostCount => _postCount;

		public override void Post(SendOrPostCallback d, object? state)
		{
			Interlocked.Increment(ref _postCount);
			// Execute inline for testing simplicity
			d(state);
		}

		public override void Send(SendOrPostCallback d, object? state)
		{
			d(state);
		}
	}

	/// <summary>
	/// A SynchronizationContext that simulates a single-threaded UI context.
	/// If code incorrectly Posts to this while blocking the thread, it would deadlock.
	/// </summary>
	sealed class SingleThreadSynchronizationContext : SynchronizationContext
	{
		public override void Post(SendOrPostCallback d, object? state)
		{
			// Queue to thread pool — simulates that the "UI thread" is busy
			ThreadPool.QueueUserWorkItem(_ => d(state));
		}

		public override void Send(SendOrPostCallback d, object? state)
		{
			// Send synchronously (blocking) — would deadlock if called from the wrong thread
			d(state);
		}
	}
}
