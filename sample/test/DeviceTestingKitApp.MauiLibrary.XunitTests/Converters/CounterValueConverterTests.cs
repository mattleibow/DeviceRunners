using System.Globalization;

using DeviceTestingKitApp.Converters;

namespace DeviceTestingKitApp.MauiLibrary.XunitTests.Converters;

public class CounterValueConverterTests
{
	[Theory]
	[InlineData(0, "Click me!")]
	[InlineData(1, "Clicked 1 time")]
	[InlineData(2, "Clicked 2 times")]
	[InlineData(3, "Clicked 3 times")]
	public void ConvertFormatsText(int number, string expectedValue)
	{
		// Arrange
		var converter = new CounterValueConverter();

		// Act
		var converted = converter.Convert(number, typeof(string), null, CultureInfo.InvariantCulture);

		// Assert
		Assert.Equal(expectedValue, converted);
	}

	[Fact]
	public void InvalidTargetTypeThrows()
	{
		var converter = new CounterValueConverter();

		Assert.Throws<NotSupportedException>(() => converter.Convert(1, typeof(bool), null, CultureInfo.InvariantCulture));
	}

	[Fact]
	public void InvalidSourceTypeThrows()
	{
		var converter = new CounterValueConverter();

		Assert.Throws<NotSupportedException>(() => converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture));
	}
}
