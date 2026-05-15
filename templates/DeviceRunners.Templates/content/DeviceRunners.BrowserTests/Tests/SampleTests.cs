namespace DeviceRunners.BrowserTests.Tests;

public class SampleTests
{
	[Fact]
	public void PassingTest()
	{
		Assert.True(true);
	}

	[Fact]
	public void StringTest()
	{
		var greeting = "Hello, Browser!";
		Assert.Contains("Browser", greeting);
	}

	[Theory]
	[InlineData(2, 3, 5)]
	[InlineData(0, 0, 0)]
	[InlineData(-1, 1, 0)]
	public void AdditionTheory(int a, int b, int expected)
	{
		Assert.Equal(expected, a + b);
	}
}
