using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightBy : IBy
{
	private Func<IPage, ILocator>? _action;

	void IBy.Selector(string selector, string value)
	{
		if (!SetBy(selector, value))
			throw new ArgumentException($"Unknown element selector '{selector}' with value '{value}'.");
	}

	protected bool SetLocator(Func<IPage, ILocator> action)
	{
		_action = action;
		return true;
	}

	protected virtual bool SetBy(string selector, string value) =>
		selector switch
		{
			BySelectors.Id => SetLocator(p => p.Locator("css=#" + value)),
			BySelectors.AccessibilityId => SetLocator(p => p.GetByTestId(value)),
			_ => false
		};

	public ILocator Locate(IPage page) => _action?.Invoke(page) ?? throw new InvalidOperationException("No element selector was specified.");
}
