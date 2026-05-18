namespace DeviceTestingKitApp.MtpDeviceTests;

/// <summary>
/// Minimal MAUI app shell for MTP device tests.
/// When DEVICE_RUNNERS_AUTORUN=1, tests run automatically and the app terminates.
/// When launched normally, shows a simple status page.
/// </summary>
public class App : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "MTP Device Tests",
						HorizontalOptions = LayoutOptions.Center,
						FontSize = 24
					},
					new Label
					{
						Text = "Use 'dotnet test' to run tests via MTP.",
						HorizontalOptions = LayoutOptions.Center,
						FontSize = 14
					}
				}
			}
		});
	}
}
