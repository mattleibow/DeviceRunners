namespace DeviceTestingKitApp.MtpDeviceTests.Tests;

/// <summary>
/// Sample tests demonstrating xunit v3 on-device tests via MTP.
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
	[InlineData(1, 1, 2)]
	[InlineData(2, 3, 5)]
	[InlineData(10, -5, 5)]
	public void AdditionTheory(int a, int b, int expected)
	{
		Assert.Equal(expected, a + b);
	}

	[Fact]
	public void StringTest()
	{
		var result = "Hello, World!";
		Assert.Contains("World", result);
	}
}
