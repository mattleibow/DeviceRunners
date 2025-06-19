namespace DeviceRunners.UIAutomation.Appium;

public class AppiumGetScreenshotCommand : AutomatedAppCommand<AppiumAutomatedApp>
{
	public AppiumGetScreenshotCommand()
		: base(CommonCommandNames.GetScreenshot)
	{
	}

	public override object? Execute(AppiumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (app.Driver.GetScreenshot() is { } screenshot)
			return new Base64StringInMemoryFile(screenshot.AsBase64EncodedString);

		return null;
	}
}
