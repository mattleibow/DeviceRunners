using DeviceTestingKitApp.Features;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Features;

public class ConsoleSemanticAnnouncerTests
{
	[Fact]
	public void AnnounceInvokesUnderlyingAnnouncer()
	{
		// Arrange
		var inner = new DummySemanticAnnouncer();
		var announcer = new DelegatingSemanticAnnouncer(inner);

		// Act
		announcer.Announce("Hello, World!");

		// Assert
		Assert.Equal(1, inner.AnnounceCount);
		var msg = Assert.Single(inner.Announcements);
		Assert.Equal("Hello, World!", msg);
	}

	[Theory]
	[InlineData("Hello, World!")]
	[InlineData("Hello, World!", "How are you?")]
	[InlineData("Hello, World!", "How are you?", "This is pretty cool.")]
	public void MultipleAnnouncementsInvokesMultipleTimes(params string[] messages)
	{
		// Arrange
		var inner = new DummySemanticAnnouncer();
		var announcer = new DelegatingSemanticAnnouncer(inner);

		// Act
		foreach (var message in messages)
		{
			announcer.Announce(message);
		}

		// Assert
		Assert.Equal(messages.Length, inner.AnnounceCount);
		Assert.Equal(messages, inner.Announcements);
	}
}

class DummySemanticAnnouncer : ISemanticAnnouncer
{
	public int AnnounceCount { get; set; }

	public List<string> Announcements { get; } = [];

	public void Announce(string message)
	{
		AnnounceCount++;
		Announcements.Add(message);
	}
}

/// <summary>
/// Simple delegating announcer — Blazor equivalent of MauiSemanticAnnouncer.
/// In production this would use JS interop for aria-live announcements.
/// </summary>
class DelegatingSemanticAnnouncer : ISemanticAnnouncer
{
	readonly ISemanticAnnouncer _inner;

	public DelegatingSemanticAnnouncer(ISemanticAnnouncer inner) => _inner = inner;

	public void Announce(string message) => _inner.Announce(message);
}
