using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Maui;

class RunStatusToColorConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not RunStatus status || Application.Current == null)
			return Colors.Red;

		return status switch
		{
			RunStatus.Passed => Application.Current.Resources["SuccessfulTestsColor"],
			RunStatus.Failed => Application.Current.Resources["FailedTestsColor"],
			RunStatus.NoTests => Application.Current.Resources["NoTestsColor"],
			RunStatus.NotRun => Application.Current.Resources["NotRunTestsColor"],
			RunStatus.Skipped => Application.Current.Resources["SkippedTestsColor"],
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotImplementedException();
}
