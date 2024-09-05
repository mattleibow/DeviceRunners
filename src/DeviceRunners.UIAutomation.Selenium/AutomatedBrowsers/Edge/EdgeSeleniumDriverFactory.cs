using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace DeviceRunners.UIAutomation.Selenium;

public class EdgeSeleniumDriverFactory : ISeleniumDriverFactory
{
	public WebDriver CreateDriver(EdgeSeleniumAutomatedAppOptions options) =>
		new EdgeDriver(options.DriverOptions);

	WebDriver ISeleniumDriverFactory.CreateDriver(SeleniumAutomatedAppOptions options) =>
		CreateDriver((EdgeSeleniumAutomatedAppOptions)options);
}
