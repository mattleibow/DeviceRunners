namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumGetScreenshotCommand : AutomatedAppCommand<SeleniumAutomatedApp>
{
	public SeleniumGetScreenshotCommand()
		: base(CommonCommandNames.GetScreenshot)
	{
	}

	public override object? Execute(SeleniumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (app.Driver.GetScreenshot() is { } screenshot)
			return new Base64StringInMemoryFile(screenshot.AsBase64EncodedString);

		return null;
	}
}
