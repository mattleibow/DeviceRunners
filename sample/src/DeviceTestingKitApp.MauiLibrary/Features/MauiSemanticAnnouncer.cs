namespace DeviceTestingKitApp.Features;

public class MauiSemanticAnnouncer : ISemanticAnnouncer
{
	private readonly ISemanticScreenReader _platformScreenReader;

	public MauiSemanticAnnouncer(ISemanticScreenReader platformScreenReader)
	{
		ArgumentNullException.ThrowIfNull(platformScreenReader);
		_platformScreenReader = platformScreenReader;
	}

	public void Announce(string message)
	{
		_platformScreenReader.Announce(message);
	}
}
