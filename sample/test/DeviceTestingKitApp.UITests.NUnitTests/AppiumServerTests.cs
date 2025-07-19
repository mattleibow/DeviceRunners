using DeviceRunners.UIAutomation.Appium;

using NUnit.Framework.Internal;

namespace DeviceTestingKitApp.UITests.NUnitTests;

public class AppiumServerTests : BaseUITests
{
	public AppiumServerTests(string appKey)
		: base(appKey)
	{
	}

	[Test]
	public void IsReady()
	{
		if (App is not AppiumAutomatedApp appiumApp)
		{
			Assert.Ignore("App was not an Appium app.");
			return;
		}

		var id = appiumApp.Driver.SessionId;

		Assert.That(id, Is.Not.Null);
		Assert.That(id.ToString(), Is.Not.Empty);
	}
}
