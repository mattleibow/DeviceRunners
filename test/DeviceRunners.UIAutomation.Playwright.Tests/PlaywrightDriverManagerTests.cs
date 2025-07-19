using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Playwright;

using Xunit;
using Xunit.Abstractions;

namespace UIAutomationPlaywrightTests;

public class PlaywrightDriverManagerTests
{
	public PlaywrightDriverManagerTests(ITestOutputHelper output)
	{
		Logger = new XunitPlaywrightDiagnosticLogger(output);
	}

	public IDiagnosticLogger Logger { get; }

	protected PlaywrightAutomatedAppOptions CreateAppOptions()
	{
		var builder = new EdgePlaywrightAutomatedAppOptionsBuilder("test");
		builder.ConfigureHeadless(false);
		return builder.Build();
	}

	[Fact]
	public void CanCreateDriver()
	{
		using var playwright = new PlaywrightServiceManager(new(), Logger);
		using var driverManager = new PlaywrightDriverManager(playwright, CreateAppOptions(), Logger);

		Assert.False(driverManager.IsRunning);
	}

	[Fact]
	public void CanCreateAndStartDriver()
	{
		using var playwright = new PlaywrightServiceManager(new(), Logger);
		using var driverManager = new PlaywrightDriverManager(playwright, CreateAppOptions(), Logger);

		driverManager.StartDriver();

		Assert.True(driverManager.IsRunning);
	}

	[Fact]
	public void CanStartWithInitialUrl()
	{
		var builder = new EdgePlaywrightAutomatedAppOptionsBuilder("test");
		builder.ConfigureHeadless(false);
		builder.UseInitialUrl("https://github.com/mattleibow/DeviceRunners");
		var options = builder.Build();

		using var playwright = new PlaywrightServiceManager(new(), Logger);
		using var driverManager = new PlaywrightDriverManager(playwright, options, Logger);
		driverManager.StartDriver();

		Assert.True(driverManager.IsRunning);
		Assert.NotNull(driverManager.Browser);
		Assert.NotNull(driverManager.Page);

		var page = driverManager.Page;
		Assert.Equal("https://github.com/mattleibow/DeviceRunners", page.Url);
	}

	[Fact]
	public async Task CanReadPageContents()
	{
		var builder = new EdgePlaywrightAutomatedAppOptionsBuilder("test");
		builder.UseInitialUrl("https://github.com/mattleibow/DeviceRunners");
		var options = builder.Build();

		using var playwright = new PlaywrightServiceManager(new(), Logger);
		using var driverManager = new PlaywrightDriverManager(playwright, options, Logger);
		driverManager.StartDriver();
		var page = driverManager.Page!;

		var element = page.Locator("css=.markdown-heading").First;
		Assert.Equal("Test Device Runners", await element.TextContentAsync());
	}
}
