using System.Runtime.CompilerServices;
using System.Xml.Linq;

using DeviceRunners.Appium;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceTestingKitApp.UITests;

[Collection(UITestsCollection.CollectionName)]
public abstract class BaseUITests : IDisposable
{
	private readonly AppiumTest _appiumTest;
	private readonly AppiumTestApp _appiumTestApp;
	private readonly ITest? _xunitTest;

	public BaseUITests(UITestsFixture fixture, ITestOutputHelper output)
	{
		_appiumTest = fixture.AppiumTest;
		_appiumTestApp = fixture.AppiumTest.GetApp(_Config.Current);

		Driver = _appiumTestApp.Driver;
		Output = output;

		DeviceBy = new UITestsDeviceBy(Driver);

		_xunitTest = GetTest(Output as TestOutputHelper);
	}

	protected AppiumDriver Driver { get; }

	protected ITestOutputHelper Output { get; }

	public void Dispose()
	{
		Output.WriteLine("Last page source:");
		if (Driver.PageSource is string source && !string.IsNullOrWhiteSpace(source))
			Output.WriteLine(XDocument.Parse(source).ToString());
		else
			Output.WriteLine("Page source is empty");

		Output.WriteLine("Last screenshot:");
		if (Driver.GetScreenshot() is { } screenshot)
			Output.WriteLine(screenshot.AsBase64EncodedString);
		else
			Output.WriteLine("No screenshot available");
	}

	protected UITestsDeviceBy DeviceBy { get; }

	protected class UITestsDeviceBy(AppiumDriver driver)
	{
		public By AutomationId(string id) =>
			driver switch
			{
				WindowsDriver => MobileBy.AccessibilityId(id),
				_ => MobileBy.Id(id)
			};
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "test")]
	static extern ref ITest GetTest(TestOutputHelper? output);
}
