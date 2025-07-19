using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumBy : IBy
{
	private By? _by;

	void IBy.Selector(string selector, string value)
	{
		if (!SetBy(selector, value))
			throw new ArgumentException($"Unknown element selector '{selector}' with value '{value}'.");
	}

	protected bool SetBy(By by)
	{
		_by = by;
		return true;
	}

	protected virtual bool SetBy(string selector, string value) =>
		selector switch
		{
			BySelectors.Id => SetBy(By.Id(value)),
			BySelectors.AccessibilityId => SetBy(By.Id(value)),
			_ => false
		};

	public By ToBy() => _by ?? throw new InvalidOperationException("No element selector was specified.");
}
