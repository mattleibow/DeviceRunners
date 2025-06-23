namespace DeviceTestingKitApp.Converters;

public static class CounterTextConverter
{
	public static string ConvertCountToText(int count)
	{
		return count switch
		{
			0 => "Click me!",
			1 => $"Clicked {count} time",
			_ => $"Clicked {count} times"
		};
	}
}