using DeviceRunners.UIAutomation.Playwright;

using Xunit;

namespace UIAutomationPlaywrightTests;

public class PlaywrightServiceManagerTests
{
	[Fact]
	public void CanStartService()
	{
		var options = new PlaywrightServiceManagerOptions()
		{
		};

		using var manager = new PlaywrightServiceManager(options);

		Assert.True(manager.IsRunning);
	}
}
