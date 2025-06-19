namespace DeviceRunners.UIAutomation.Appium;

public class WindowsAppiumBy : AppiumBy
{
	protected override bool SetBy(string selector, string value)
	{
		// Windows does not use ID but rather Accessibility ID to find elements
		if (selector == BySelectors.Id)
			selector = BySelectors.AccessibilityId;

		return base.SetBy(selector, value);
	}
}
