namespace DeviceTestingKitApp.Features;

/// <summary>
/// Blazor implementation of ISemanticAnnouncer.
/// In production, would use JS interop to set aria-live region text.
/// For testing, wraps an inner announcer (dependency injection).
/// </summary>
public class BlazorSemanticAnnouncer : ISemanticAnnouncer
{
	readonly Action<string> _announce;

	/// <summary>
	/// Creates an announcer that writes to console (fallback when no JS runtime).
	/// </summary>
	public BlazorSemanticAnnouncer()
	{
		_announce = Console.WriteLine;
	}

	/// <summary>
	/// Creates an announcer with a custom announce action (for JS interop or testing).
	/// </summary>
	public BlazorSemanticAnnouncer(Action<string> announce)
	{
		_announce = announce ?? throw new ArgumentNullException(nameof(announce));
	}

	public void Announce(string message) => _announce(message);
}
