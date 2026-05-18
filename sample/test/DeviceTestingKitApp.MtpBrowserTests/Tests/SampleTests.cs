namespace DeviceTestingKitApp.MtpBrowserTests.Tests;

/// <summary>
/// Sample tests demonstrating xunit v3 browser tests via MTP.
/// These tests live in the app project (single-assembly model).
/// </summary>
public class SampleTests
{
	[Fact]
	public void PassingTest()
	{
		Assert.True(true);
	}

	[Fact]
	public void MathTest()
	{
		Assert.Equal(4, 2 + 2);
	}

	[Theory]
	[InlineData("hello", 5)]
	[InlineData("world", 5)]
	[InlineData("", 0)]
	public void StringLengthTheory(string input, int expectedLength)
	{
		Assert.Equal(expectedLength, input.Length);
	}

	[Fact]
	public async Task AsyncTestWorks()
	{
		await Task.Delay(50);
		Assert.True(true);
	}
}
