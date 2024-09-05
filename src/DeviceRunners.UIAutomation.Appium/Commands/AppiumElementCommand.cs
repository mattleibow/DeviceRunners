using OpenQA.Selenium.Appium;

namespace DeviceRunners.UIAutomation.Appium;

public abstract class AppiumElementCommand : AutomatedAppCommand<AppiumAutomatedApp>
{
	protected AppiumElementCommand(string name)
		: base(name)
	{
	}

	public abstract object? Execute(AppiumAutomatedApp app, AppiumElement appiumElement, IReadOnlyDictionary<string, object> parameters);

	public override object? Execute(AppiumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (parameters is null || !parameters.TryGetValue("element", out var element))
			throw new ArgumentException("Element not found in parameters", nameof(parameters));

		if (element is not AppiumAutomatedAppElement appiumElement)
			throw new ArgumentException("Element is not an Appium element", nameof(parameters));

		return Execute(app, appiumElement.AppiumElement, parameters);
	}
}
