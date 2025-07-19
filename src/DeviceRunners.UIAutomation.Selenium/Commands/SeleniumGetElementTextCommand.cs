using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumGetElementTextCommand : SeleniumElementCommand
{
	public SeleniumGetElementTextCommand()
		: base(CommonCommandNames.GetElementText)
	{
	}

	public override object? Execute(SeleniumAutomatedApp app, WebElement seleniumElement, IReadOnlyDictionary<string, object> parameters) =>
		seleniumElement.Text;
}
