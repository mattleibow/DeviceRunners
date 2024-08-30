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

	[Fact]
	public void IsReady()
	{
		if (App is not AppiumAutomatedApp appiumApp)
			return;

		var id = appiumApp.Driver.SessionId;

		Assert.NotNull(id);
		Assert.NotEmpty(id.ToString());
	}
}
