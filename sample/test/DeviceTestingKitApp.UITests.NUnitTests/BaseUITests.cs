using System.Xml.Linq;

using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Appium;

using NUnit.Framework.Internal;

namespace DeviceTestingKitApp.UITests.NUnitTests;

[TestFixture("android")]
[TestFixture("windows")]
[Parallelizable(ParallelScope.All)]
public abstract class BaseUITests
{
	private readonly string _testKey;

	private AutomationTestSuite _automationTestSuite;
	private IAutomatedApp _app;

	public BaseUITests(string testKey)
	{
		_testKey = testKey;
	}

	protected IAutomatedApp App => _app;

	protected TextWriter Output => TestContext.Out;

	[SetUp]
	public void SetUp()
	{
		_automationTestSuite = UITestsSetupFixture.TestSuite;
		_app = _automationTestSuite.StartApp(_testKey);

		//DeviceBy = new UITestsDeviceBy(Driver);
	}

	[TearDown]
	public void TearDown()
	{
		if (App?.GetPageSource() is string source && !string.IsNullOrWhiteSpace(source))
			Output.WriteLine($"Last page source:{Environment.NewLine}{XDocument.Parse(source)}");
		else
			Output.WriteLine("Page source is empty");

		if (App?.GetScreenshot() is { } screenshot)
			Output.WriteLine($"Last screenshot:{Environment.NewLine}{screenshot.ToBase64String()}");
		else
			Output.WriteLine("No screenshot available");

		_automationTestSuite.StopApp(_testKey);
	}

	//protected UITestsDeviceBy DeviceBy { get; }

	//protected class UITestsDeviceBy(AppiumDriver driver)
	//{
	//	public By AutomationId(string id) =>
	//		driver switch
	//		{
	//			WindowsDriver => MobileBy.AccessibilityId(id),
	//			_ => MobileBy.Id(id)
	//		};
	//}
}
