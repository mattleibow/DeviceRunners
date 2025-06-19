namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumByFactory : AppiumByFactory
{
	public override AppiumBy Create(AppiumAutomatedApp app) => new WindowsAppiumBy();
}
