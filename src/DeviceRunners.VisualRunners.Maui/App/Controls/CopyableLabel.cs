using CommunityToolkit.Maui.Behaviors;

namespace DeviceRunners.VisualRunners.Maui.App.Controls;

public class CopyableLabel : Label
{
	public CopyableLabel()
	{
		// 1) Create a TouchBehavior that fires after 600ms of press
		var touch = new TouchBehavior
		{
			LongPressDuration = 600,  // duration threshold in milliseconds
			// When the long press completes, run this command:
			LongPressCommand = new Command(async () => await CopyTextAsync())
		};

		// 2) Attach it so every instance of CopyableLabel has "long-press to copy"
		Behaviors.Add(touch);
	}

	// Copies this.Text to the system clipboard
	async Task CopyTextAsync()
	{
		if (string.IsNullOrWhiteSpace(Text))
			return;

		await Clipboard.Default.SetTextAsync(Text);
		// (Optional) You could display a "Copied!" snackbar or toast here.
		await CommunityToolkit.Maui.Alerts.Snackbar.Make("Copied to clipboard!").Show();
	}
}
