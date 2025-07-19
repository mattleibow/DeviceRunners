namespace DeviceRunners.UIAutomation.Appium;

public interface IAppiumByFactory
{
	AppiumBy Create(AppiumAutomatedApp app);
}
