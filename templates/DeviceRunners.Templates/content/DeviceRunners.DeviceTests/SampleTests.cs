namespace DeviceRunners.DeviceTests;

public class SampleTests
{
	[Fact]
	public void PassingTest()
	{
		Assert.True(true);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void ParameterizedTest(int value)
	{
		Assert.NotEqual(0, value);
	}

	[Fact]
	public async Task AsyncTest()
	{
		await Task.Delay(100);
		Assert.True(true);
	}
}
