using DeviceRunners.Appium;

using OpenQA.Selenium.Appium.Enums;

using Xunit;
using Xunit.Abstractions;

namespace DeviceTestingKitApp.UITests;

public class AppiumServerTests : BaseUITests
{
	public AppiumServerTests(UITestsFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[Fact]
	public void IsReady()
	{
		var id = Driver.SessionId;

		Assert.NotNull(id);
		Assert.NotEmpty(id.ToString());

		Assert.Equal(AppState.RunningInForeground, Driver.GetAppState());
	}
}
