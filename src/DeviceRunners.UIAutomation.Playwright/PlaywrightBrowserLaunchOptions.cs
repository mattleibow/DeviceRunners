namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightBrowserLaunchOptions : Dictionary<string, object>, IPlaywrightBrowserLaunchOptions
{
	object? IPlaywrightBrowserLaunchOptions.this[string name] => Get(name);

	object? IPlaywrightBrowserLaunchOptions.GetOption(string name) => Get(name);

	bool IPlaywrightBrowserLaunchOptions.HasOption(string name) => ContainsKey(name);

	private object? Get(string name) =>
		TryGetValue(name, out var value)
			? value
			: null;
}
