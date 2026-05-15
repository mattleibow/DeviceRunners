using System.Runtime.InteropServices.JavaScript;

namespace DeviceRunners.VisualRunners.Blazor;

/// <summary>
/// Low-level JavaScript interop for reading browser state.
/// Used internally by the Blazor visual test runner.
/// </summary>
static partial class BrowserInterop
{
	/// <summary>
	/// Returns the current page URL (window.location.href).
	/// Requires a <c>getLocationHref</c> function to be defined in the host page.
	/// </summary>
	[JSImport("globalThis.getLocationHref")]
	internal static partial string GetLocationHref();
}
