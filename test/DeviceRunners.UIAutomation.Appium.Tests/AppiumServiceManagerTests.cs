using DeviceRunners.UIAutomation.Appium;

using Xunit;

namespace UIAutomationAppiumTests;

public class AppiumServiceManagerTests
{
	[Fact]
	public void HostAddressAndPortAreUsed()
	{
		var options = new AppiumServiceManagerOptions()
		{
			HostAddress = "127.0.0.1",
			HostPort = 2468,
		};

		using var manager = new AppiumServiceManager(options);

		Assert.True(manager.IsRunning);
		Assert.Equal(new Uri("http://127.0.0.1:2468/"), manager.ServiceUrl);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(-2)]
	public void NegativeOrZeroPortMeansRandomPort(int port)
	{
		var options = new AppiumServiceManagerOptions()
		{
			HostAddress = "127.0.0.1",
			HostPort = port,
		};

		using var manager = new AppiumServiceManager(options);

		Assert.True(manager.IsRunning);
		Assert.Equal("127.0.0.1", manager.ServiceUrl.Host);
		Assert.True(manager.ServiceUrl.Port > 0, $"Port '{manager.ServiceUrl.Port}' was not a valid free port when '{port}' was specified.");
	}
}
