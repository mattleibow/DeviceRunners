using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public abstract class SeleniumElementCommand : AutomatedAppCommand<SeleniumAutomatedApp>
{
	protected SeleniumElementCommand(string name)
		: base(name)
	{
	}

	public abstract object? Execute(SeleniumAutomatedApp app, WebElement seleniumElement, IReadOnlyDictionary<string, object> parameters);

	public override object? Execute(SeleniumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (parameters is null || !parameters.TryGetValue("element", out var element))
			throw new ArgumentException("Element not found in parameters", nameof(parameters));

		if (element is not SeleniumAutomatedAppElement seleniumElement)
			throw new ArgumentException("Element is not an Selenium element", nameof(parameters));

		return Execute(app, seleniumElement.SeleniumElement, parameters);
	}
}
