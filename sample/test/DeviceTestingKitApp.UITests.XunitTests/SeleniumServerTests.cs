using DeviceRunners.UIAutomation.Selenium;

using Xunit;
using Xunit.Abstractions;

namespace DeviceTestingKitApp.UITests.XunitTests;

public class SeleniumServerTests : BaseUITests
{
	public SeleniumServerTests(UITestsFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[SkippableFact]
	public void IsReady()
	{
		if (App is not SeleniumAutomatedApp seleniumApp)
		{
			Skip.If(true, "App was not an Selenium app.");
			return;
		}

		var id = seleniumApp.Driver.SessionId;

		Assert.NotNull(id);
		Assert.NotEmpty(id.ToString());
	}
}
