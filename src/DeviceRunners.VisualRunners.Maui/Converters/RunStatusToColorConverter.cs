using System.Globalization;

namespace DeviceRunners.VisualRunners.Maui;

class RunStatusToColorConverter : IValueConverter
{
	static readonly Color SuccessfulTestsColor = Colors.Green;
	static readonly Color FailedTestsColor = Colors.Red;
	static readonly Color NoTestsColor = Color.FromArgb("#ff7f00");
	static readonly Color NotRunTestsColor = Colors.DarkGray;
	static readonly Color SkippedTestsColor = Color.FromArgb("#ff7700");

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not TestResultStatus status)
			return Colors.Red;

		return status switch
		{
			TestResultStatus.Passed => SuccessfulTestsColor,
			TestResultStatus.Failed => FailedTestsColor,
			TestResultStatus.NoTests => NoTestsColor,
			TestResultStatus.NotRun => NotRunTestsColor,
			TestResultStatus.Skipped => SkippedTestsColor,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotImplementedException();
}
