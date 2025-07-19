using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public abstract class PlaywrightElementCommand : AutomatedAppCommand<PlaywrightAutomatedApp>
{
	protected PlaywrightElementCommand(string name)
		: base(name)
	{
	}

	public abstract object? Execute(PlaywrightAutomatedApp app, ILocator playwrightElement, IReadOnlyDictionary<string, object> parameters);

	public override object? Execute(PlaywrightAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (parameters is null || !parameters.TryGetValue("element", out var element))
			throw new ArgumentException("Element not found in parameters", nameof(parameters));

		if (element is not PlaywrightAutomatedAppElement playwrightElement)
			throw new ArgumentException("Element is not an Playwright element", nameof(parameters));

		return Execute(app, playwrightElement.PlaywrightElement, parameters);
	}
}
