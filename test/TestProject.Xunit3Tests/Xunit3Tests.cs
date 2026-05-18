using Xunit;

namespace TestProject.Xunit3Tests;

public class Xunit3Tests : IDisposable
{
	public Xunit3Tests()
	{
	}

	public void Dispose()
	{
	}

	[Fact]
	public void SimpleTest()
	{
		Assert.True(true);
	}

	[Fact]
	public void SimpleTest_Failed()
	{
		throw new Exception(Constants.ErrorMessage);
	}

	[Fact(Skip = Constants.SkippedReason)]
	public void SimpleTest_Skipped()
	{
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void DataTest(int number)
	{
		Assert.True(true);
	}
}

/// <summary>
/// Tests that <see cref="IAsyncLifetime"/> is properly handled during test execution.
/// This guards against regressions in the UI thread dispatch pipeline — the v3
/// <c>UIXunitTestRunner.RunTest</c> override must dispatch the entire lifecycle
/// (including InitializeAsync/DisposeAsync) to the UI thread.
/// </summary>
public class Xunit3AsyncLifetimeTests : IAsyncLifetime
{
	bool _initialized;

	public ValueTask InitializeAsync()
	{
		_initialized = true;
		return ValueTask.CompletedTask;
	}

	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}

	[Fact]
	public void InitializeAsync_WasCalled()
	{
		Assert.True(_initialized);
	}

	[Fact]
	public void SimpleAsyncLifetimeTest()
	{
		Assert.True(true);
	}
}
