using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public class AppiumGetElementTextCommand : AppiumElementCommand
{
	public AppiumGetElementTextCommand()
		: base(CommonCommandNames.GetElementText)
	{
	}

	public override object? Execute(AppiumAutomatedApp app, AppiumElement appiumElement, IReadOnlyDictionary<string, object> parameters) =>
		appiumElement.Text;
}
