using DeviceRunners.UIAutomation.Appium;

using Xunit;
using Xunit.Abstractions;

namespace DeviceTestingKitApp.UITests.XunitTests;

public class AppiumServerTests : BaseUITests
{
	public AppiumServerTests(UITestsFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[SkippableFact]
	public void IsReady()
	{
		if (App is not AppiumAutomatedApp appiumApp)
		{
			Skip.If(true, "App was not an Appium app.");
			return;
		}

		var id = appiumApp.Driver.SessionId;

		Assert.NotNull(id);
		Assert.NotEmpty(id.ToString());
	}
}
