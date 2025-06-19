using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public interface ISeleniumDriverFactory
{
	WebDriver CreateDriver(SeleniumAutomatedAppOptions options);
}
