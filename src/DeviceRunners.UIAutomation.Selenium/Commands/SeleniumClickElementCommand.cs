using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumClickElementCommand : SeleniumElementCommand
{
	public SeleniumClickElementCommand()
		: base(CommonCommandNames.ClickElement)
	{
	}

	public override object? Execute(SeleniumAutomatedApp app, WebElement seleniumElement, IReadOnlyDictionary<string, object> parameters)
	{
		try
		{
			seleniumElement.Click();
			return null;
		}
		catch (Exception ex) when (ex is WebDriverException || ex is InvalidOperationException)
		{
			// Some elements aren't "clickable" from an automation perspective, such
			// as borders and labels. In this case, we can try to tap the element using
			// its center point - which is what the click does anyway.

			var pointString = seleniumElement.GetAttribute("ClickablePoint");
			var parts = pointString.Split(',');
			double x = double.Parse(parts[0]);
			double y = double.Parse(parts[1]);

			return app.Commands.Execute(CommonCommandNames.ClickCoordinates, ("x", x), ("y", y));
		}
	}
}
