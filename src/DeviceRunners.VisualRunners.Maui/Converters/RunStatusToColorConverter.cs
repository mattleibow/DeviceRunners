using System.Globalization;

namespace DeviceRunners.VisualRunners.Maui;

class RunStatusToColorConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not TestResultStatus status || Application.Current == null)
			return Colors.Red;

		return status switch
		{
			TestResultStatus.Passed => Application.Current.Resources["SuccessfulTestsColor"],
			TestResultStatus.Failed => Application.Current.Resources["FailedTestsColor"],
			TestResultStatus.NoTests => Application.Current.Resources["NoTestsColor"],
			TestResultStatus.NotRun => Application.Current.Resources["NotRunTestsColor"],
			TestResultStatus.Skipped => Application.Current.Resources["SkippedTestsColor"],
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotImplementedException();
}
