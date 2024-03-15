using System.Globalization;

namespace DeviceTestingKitApp.Converters;

public class CounterValueConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not int count || targetType != typeof(string))
			throw new NotSupportedException();

		return count switch
		{
			0 => "Click me!",
			1 => $"Clicked {count} time",
			_ => $"Clicked {count} times"
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
