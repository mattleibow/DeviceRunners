using DeviceRunners.UIAutomation.Appium;

using Xunit;

namespace UIAutomationAppiumTests;

public class AppiumDriverManagerTests
{
	protected AppiumAutomatedAppOptions CreateAppOptions()
	{
		var builder = new WindowsAppiumAutomatedAppOptionsBuilder("test");
		return builder.Build();
	}

	[Fact]
	public void CanCreateDriver()
	{
		using var appium = new AppiumServiceManager(new());
		using var driverManager = new AppiumDriverManager(appium, CreateAppOptions());

		Assert.False(driverManager.IsRunning);
	}

	[Fact]
	public void CanCreateAndStartDriver()
	{
		using var appium = new AppiumServiceManager(new());
		using var driverManager = new AppiumDriverManager(appium, CreateAppOptions());

		driverManager.StartDriver();

		Assert.True(driverManager.IsRunning);
	}
}
