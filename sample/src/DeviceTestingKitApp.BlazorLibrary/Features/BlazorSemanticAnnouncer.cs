using DeviceTestingKitApp.Features;
using Microsoft.JSInterop;

namespace DeviceTestingKitApp.Features;

public class BlazorSemanticAnnouncer : ISemanticAnnouncer, IAsyncDisposable
{
	private readonly IJSRuntime _jsRuntime;
	private IJSObjectReference? _module;

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
				await EnsureModuleLoadedAsync();
				if (_module != null)
				{
					await _module.InvokeVoidAsync("announceToScreenReader", message);
				}
			}
			catch
			{
				// Ignore JS interop errors in case JS is not available
			}
		});
	}

	private async Task EnsureModuleLoadedAsync()
	{
		if (_module == null)
		{
			_module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/DeviceTestingKitApp.BlazorLibrary/js/accessibility.js");
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_module != null)
		{
			await _module.DisposeAsync();
		}
	}
}