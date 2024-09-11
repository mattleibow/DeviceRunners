//using OpenQA.Selenium;
//using OpenQA.Selenium.Playwright;

//namespace DeviceRunners.UIAutomation.Playwright;

//public class PlaywrightClickElementCommand : PlaywrightElementCommand
//{
//	public PlaywrightClickElementCommand()
//		: base(CommonCommandNames.ClickElement)
//	{
//	}

//	public override object? Execute(PlaywrightAutomatedApp app, PlaywrightElement playwrightElement, IReadOnlyDictionary<string, object> parameters)
//	{
//		try
//		{
//			playwrightElement.Click();
//			return null;
//		}
//		catch (Exception ex) when (ex is WebDriverException || ex is InvalidOperationException)
//		{
//			// Some elements aren't "clickable" from an automation perspective, such
//			// as borders and labels. In this case, we can try to tap the element using
//			// its center point - which is what the click does anyway.

//			var pointString = playwrightElement.GetAttribute("ClickablePoint");
//			var parts = pointString.Split(',');
//			double x = double.Parse(parts[0]);
//			double y = double.Parse(parts[1]);

//			return app.Commands.Execute(CommonCommandNames.ClickCoordinates, ("x", x), ("y", y));
//		}
//	}
//}
