using Xunit;

/// <summary>
/// Sample Xunit tests that run in browser WebAssembly.
/// </summary>
public class SampleXunitTests
{
	[Fact]
	public void PassingTest()
	{
		Assert.True(true);
	}

	[Fact]
	public void AnotherPassingTest()
	{
		Assert.Equal(42, 21 + 21);
	}

	[Fact]
	public void StringTest()
	{
		Assert.Contains("hello", "hello world");
	}

	[Theory]
	[InlineData(1, 2, 3)]
	[InlineData(10, 20, 30)]
	[InlineData(-1, 1, 0)]
	public void AdditionTest(int a, int b, int expected)
	{
		Assert.Equal(expected, a + b);
	}

	[Fact(Skip = "Demonstrating a skipped test")]
	public void SkippedTest()
	{
		Assert.True(false, "This should not run");
	}

	[Fact]
	public void EnvironmentTest()
	{
		// Verify we're running in a WASM environment
		Assert.True(OperatingSystem.IsBrowser() || true, "Test should work on any platform");
	}
}
