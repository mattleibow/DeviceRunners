namespace DeviceRunners.UIAutomation.Playwright;

public interface IPlaywrightBrowserLaunchOptions
{
	object? this[string name] { get; }
	
	bool HasOption(string name);
	
	object? GetOption(string name);
}
