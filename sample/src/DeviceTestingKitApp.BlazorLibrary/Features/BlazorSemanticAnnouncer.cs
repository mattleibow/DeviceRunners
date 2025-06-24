using DeviceTestingKitApp.Features;
using Microsoft.JSInterop;

namespace DeviceTestingKitApp.Features;

public class BlazorSemanticAnnouncer : ISemanticAnnouncer
{
	private readonly IJSRuntime _jsRuntime;

	public BlazorSemanticAnnouncer(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public void Announce(string message)
	{
		// Use ARIA live regions for proper accessibility announcements
		_ = Task.Run(async () =>
		{
			try
			{
				await _jsRuntime.InvokeVoidAsync("announceToScreenReader", message);
			}
			catch
			{
				// Ignore JS interop errors in case JS is not available
			}
		});
	}
}