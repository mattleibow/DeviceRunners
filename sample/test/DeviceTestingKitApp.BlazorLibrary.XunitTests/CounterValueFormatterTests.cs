using DeviceTestingKitApp.Formatters;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests;

public class CounterValueFormatterTests
{
	[Fact]
	public void Format_Zero_ReturnsClickMe()
	{
		Assert.Equal("Click me!", CounterValueFormatter.Format(0));
	}

	[Fact]
	public void Format_One_ReturnsSingular()
	{
		Assert.Equal("Clicked 1 time", CounterValueFormatter.Format(1));
	}

	[Theory]
	[InlineData(2, "Clicked 2 times")]
	[InlineData(5, "Clicked 5 times")]
	[InlineData(100, "Clicked 100 times")]
	public void Format_Multiple_ReturnsPlural(int count, string expected)
	{
		Assert.Equal(expected, CounterValueFormatter.Format(count));
	}
}
