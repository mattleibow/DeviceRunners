namespace DeviceRunners.UIAutomation.Appium;

public class AppiumByFactory : IAppiumByFactory
{
	public virtual AppiumBy Create(AppiumAutomatedApp app) => new AppiumBy();
}
