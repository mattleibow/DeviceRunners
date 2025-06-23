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
		// In a web context, we can use ARIA live regions or console for accessibility
		// For simplicity, we'll use console.log, but in a real app you might want to use ARIA live regions
		_ = Task.Run(async () =>
		{
			try
			{
				await _jsRuntime.InvokeVoidAsync("console.log", $"Accessibility announcement: {message}");
			}
			catch
			{
				// Ignore JS interop errors in case JS is not available
			}
		});
	}
}