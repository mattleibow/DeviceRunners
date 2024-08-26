using System.Runtime.CompilerServices;
using System.Xml.Linq;

using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Appium;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceTestingKitApp.UITests;

[Collection(UITestsCollection.CollectionName)]
public abstract class BaseUITests : IDisposable
{
	private readonly AutomationTestSuite _automationTestSuite;
	private readonly ITest? _xunitTest;

	public BaseUITests(UITestsFixture fixture, ITestOutputHelper output)
	{
		_automationTestSuite = fixture.TestSuite;
		App = _automationTestSuite.StartApp(_Config.Current);
		Output = output;

		DeviceBy = new UITestsDeviceBy(Driver);

		_xunitTest = GetTest(Output as TestOutputHelper);
	}

	protected IAutomatedApp App { get; }

	protected ITestOutputHelper Output { get; }

	public void Dispose()
	{
		Output.WriteLine("Last page source:");
		if (App.GetPageSource() is string source && !string.IsNullOrWhiteSpace(source))
			Output.WriteLine(XDocument.Parse(source).ToString());
		else
			Output.WriteLine("Page source is empty");

		Output.WriteLine("Last screenshot:");
		if (App.GetScreenshot() is { } screenshot)
		{
			Output.WriteLine(screenshot.ToBase64String());
		}
		else
		{
			Output.WriteLine("No screenshot available");
		}

		_automationTestSuite.StopApp(_Config.Current);
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
