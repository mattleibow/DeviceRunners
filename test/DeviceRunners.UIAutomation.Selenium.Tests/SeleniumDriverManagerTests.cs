using DeviceRunners.UIAutomation.Selenium;

using OpenQA.Selenium;

using Xunit;

namespace UIAutomationSeleniumTests;

public class SeleniumDriverManagerTests
{
	protected SeleniumAutomatedAppOptions CreateAppOptions()
	{
		var builder = new EdgeSeleniumAutomatedAppOptionsBuilder("test");
		return builder.Build();
	}

	[Fact]
	public void CanCreateDriver()
	{
		using var driverManager = new SeleniumDriverManager(CreateAppOptions());

		Assert.False(driverManager.IsRunning);
	}

	[Fact]
	public void CanCreateAndStartDriver()
	{
		using var driverManager = new SeleniumDriverManager(CreateAppOptions());

		driverManager.StartDriver();

		Assert.True(driverManager.IsRunning);
	}

	[Fact]
	public void CanStartWithInitialUrl()
	{
		var builder = new EdgeSeleniumAutomatedAppOptionsBuilder("test");
		builder.UseInitialUrl("https://github.com/mattleibow/DeviceRunners");
		var options = builder.Build();

		using var driverManager = new SeleniumDriverManager(options);
		driverManager.StartDriver();

		Assert.True(driverManager.IsRunning);
		Assert.NotNull(driverManager.Driver);

		var driver = driverManager.Driver;
		Assert.Equal("https://github.com/mattleibow/DeviceRunners", driver.Url);
	}

	[Fact]
	public void CanReadPageContents()
	{
		var builder = new EdgeSeleniumAutomatedAppOptionsBuilder("test");
		builder.UseInitialUrl("https://github.com/mattleibow/DeviceRunners");
		var options = builder.Build();

		using var driverManager = new SeleniumDriverManager(options);
		driverManager.StartDriver();
		var driver = driverManager.Driver!;

		var element = driver.FindElement(By.ClassName("markdown-heading"));
		Assert.Equal("Test Device Runners", element.Text);
	}
}
