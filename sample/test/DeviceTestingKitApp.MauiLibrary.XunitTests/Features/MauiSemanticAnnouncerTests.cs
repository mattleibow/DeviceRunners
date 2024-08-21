using DeviceTestingKitApp.Features;

namespace DeviceTestingKitApp.MauiLibrary.XunitTests.Features;

public class MauiSemanticAnnouncerTests
{
	[Fact]
	public void AnnounceInvokesPlatformSemanticScreenReader()
	{
		// Arrange
		var semanticScreenReader = new DummySemanticScreenReader();
		var announcer = new MauiSemanticAnnouncer(semanticScreenReader);

		// Act
		announcer.Announce("Hello, World!");

		// Assert
		Assert.Equal(1, semanticScreenReader.AnnounceCount);
		var msg = Assert.Single(semanticScreenReader.Announcements);
		Assert.Equal("Hello, World!", msg);
	}

	[Theory]
	[InlineData("Hello, World!")]
	[InlineData("Hello, World!", "How are you?")]
	[InlineData("Hello, World!", "How are you?", "This is pretty cool.")]
	public void MultipleAnnouncementsInvokesPlatformSemanticScreenReaderMultipleTimes(params string[] messages)
	{
		// Arrange
		var semanticScreenReader = new DummySemanticScreenReader();
		var announcer = new MauiSemanticAnnouncer(semanticScreenReader);

		// Act
		foreach (var message in messages)
		{
			announcer.Announce(message);
		}

		// Assert
		Assert.Equal(messages.Length, semanticScreenReader.AnnounceCount);
		Assert.Equal(messages, semanticScreenReader.Announcements);
	}
}

class DummySemanticScreenReader : ISemanticScreenReader
{
	public int AnnounceCount { get; set; }

	public List<string> Announcements { get; } = [];

	public void Announce(string message)
	{
		AnnounceCount++;

		Announcements.Add(message);
	}
}
