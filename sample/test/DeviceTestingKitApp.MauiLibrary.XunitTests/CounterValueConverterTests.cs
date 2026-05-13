using System.Globalization;

using DeviceTestingKitApp.Converters;

namespace DeviceTestingKitApp.MauiLibrary.XunitTests;

public class CounterValueConverterTests
{
	readonly CounterValueConverter _converter = new();

	[Fact]
	public void Convert_Zero_ReturnsClickMe()
	{
		var result = _converter.Convert(0, typeof(string), null, CultureInfo.InvariantCulture);
		Assert.Equal("Click me!", result);
	}

	[Fact]
	public void Convert_One_ReturnsSingular()
	{
		var result = _converter.Convert(1, typeof(string), null, CultureInfo.InvariantCulture);
		Assert.Equal("Clicked 1 time", result);
	}

	[Theory]
	[InlineData(2, "Clicked 2 times")]
	[InlineData(5, "Clicked 5 times")]
	[InlineData(100, "Clicked 100 times")]
	public void Convert_Multiple_ReturnsPlural(int count, string expected)
	{
		var result = _converter.Convert(count, typeof(string), null, CultureInfo.InvariantCulture);
		Assert.Equal(expected, result);
	}
}
