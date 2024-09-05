namespace DeviceRunners.UIAutomation.Selenium;

public interface ISeleniumByFactory
{
	SeleniumBy Create(SeleniumAutomatedApp app);
}
