namespace DeviceTestingKitApp.DeviceTests;

[Collection("UITests")]
public class AsyncLifetimeTests : IAsyncLifetime
{
	static int _activeInstances;
	bool _initialized;

	public async Task InitializeAsync()
	{
		var current = Interlocked.Increment(ref _activeInstances);
		Assert.Equal(1, current);

		_initialized = true;
		await Task.Yield();
	}

	public async Task DisposeAsync()
	{
		Interlocked.Decrement(ref _activeInstances);
		await Task.CompletedTask;
	}

	[Fact]
	public void InitializeAsyncRanBeforeTest()
	{
		Assert.True(_initialized);
	}

	[Fact]
	public async Task AsyncSetupAndTeardownWork()
	{
		Assert.True(_initialized);
		await Task.Delay(10);
	}
}

[Collection("UITests")]
public class AsyncLifetimeTests2 : IAsyncLifetime
{
	static int _setupCount;

	public Task InitializeAsync()
	{
		Interlocked.Increment(ref _setupCount);
		return Task.CompletedTask;
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void SetupRunsForEachTestClass()
	{
		Assert.True(_setupCount > 0);
	}
}
