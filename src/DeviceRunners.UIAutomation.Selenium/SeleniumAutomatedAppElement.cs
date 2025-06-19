using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumAutomatedAppElement : IAutomatedAppElement
{
	public SeleniumAutomatedAppElement(SeleniumAutomatedApp app, WebElement element)
	{
		App = app;
		SeleniumElement = element;
	}

	public SeleniumAutomatedApp App { get; }

	public WebElement SeleniumElement { get; }

	IAutomatedApp IAutomatedAppElement.App => App;
}
