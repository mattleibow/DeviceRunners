using DeviceRunners.UIAutomation.Selenium;

using NUnit.Framework.Internal;

namespace DeviceTestingKitApp.UITests.NUnitTests;

public class SeleniumServerTests : BaseUITests
{
	public SeleniumServerTests(string appKey)
		: base(appKey)
	{
	}

	[Test]
	public void IsReady()
	{
		if (App is not SeleniumAutomatedApp seleniumApp)
		{
			Assert.Ignore("App was not a Selenium app.");
			return;
		}

		var id = seleniumApp.Driver.SessionId;

		Assert.That(id, Is.Not.Null);
		Assert.That(id.ToString(), Is.Not.Empty);
	}
}
