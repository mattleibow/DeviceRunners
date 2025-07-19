namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumByFactory : ISeleniumByFactory
{
	public virtual SeleniumBy Create(SeleniumAutomatedApp app) => new SeleniumBy();
}
