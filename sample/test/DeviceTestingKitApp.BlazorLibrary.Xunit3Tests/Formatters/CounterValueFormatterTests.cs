using DeviceTestingKitApp.Formatters;
using Xunit;

namespace DeviceTestingKitApp.BlazorLibrary.Xunit3Tests.Formatters;

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

	[Theory]
	[InlineData(-1, "Clicked -1 times")]
	[InlineData(2147483647, "Clicked 2147483647 times")]
	public void Format_EdgeCases_ReturnsPlural(int count, string expected)
	{
		Assert.Equal(expected, CounterValueFormatter.Format(count));
	}
}
