using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class AppiumAutomatedAppElement : IAutomatedAppElement
{
	public AppiumAutomatedAppElement(AppiumAutomatedApp app, AppiumElement element)
	{
		App = app;
		AppiumElement = element;
	}

	public AppiumAutomatedApp App { get; }

	public AppiumElement AppiumElement { get; }

	IAutomatedApp IAutomatedAppElement.App => App;
}
