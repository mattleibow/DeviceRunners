using DeviceRunners.UIAutomation.Appium;

using Xunit;

namespace UIAutomationAppiumTests;

public class AppiumDriverManagerTests
{
	protected AppiumAutomatedAppOptions CreateAppOptions()
	{
		var key = "test";
		var builder = OperatingSystem.IsWindows()
			? (AppiumAutomatedAppOptionsBuilder)new WindowsAppiumAutomatedAppOptionsBuilder(key)
			: (AppiumAutomatedAppOptionsBuilder)new AndroidAppiumAutomatedAppOptionsBuilder(key);
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
		if (!OperatingSystem.IsWindows())
			return;

		using var appium = new AppiumServiceManager(new());
		var builder = new WindowsAppiumAutomatedAppOptionsBuilder("test");
		builder.UseAppExecutablePath("C:\\Windows\\System32\\notepad.exe");
		using var driverManager = new AppiumDriverManager(appium, builder.Build());

		driverManager.StartDriver();

		Assert.True(driverManager.IsRunning);
	}
}
