using DeviceTestingKitApp.Features;

using Xunit;

namespace DeviceTestingKitApp.BlazorLibrary.Xunit3Tests.Features;

public class BlazorSemanticAnnouncerTests
{
	[Fact]
	public void AnnounceInvokesAction()
	{
		var messages = new List<string>();
		var announcer = new BlazorSemanticAnnouncer(msg => messages.Add(msg));

		announcer.Announce("Hello, World!");

		var msg = Assert.Single(messages);
		Assert.Equal("Hello, World!", msg);
	}

	[Theory]
	[InlineData("Hello, World!")]
	[InlineData("Hello, World!", "How are you?")]
	[InlineData("Hello, World!", "How are you?", "This is pretty cool.")]
	public void MultipleAnnouncementsInvokesMultipleTimes(params string[] expectedMessages)
	{
		var messages = new List<string>();
		var announcer = new BlazorSemanticAnnouncer(msg => messages.Add(msg));

		foreach (var message in expectedMessages)
		{
			announcer.Announce(message);
		}

		Assert.Equal(expectedMessages.Length, messages.Count);
		Assert.Equal(expectedMessages, messages);
	}

	[Fact]
	public void NullActionThrows()
	{
		Assert.Throws<ArgumentNullException>(() => new BlazorSemanticAnnouncer(null!));
	}
}
