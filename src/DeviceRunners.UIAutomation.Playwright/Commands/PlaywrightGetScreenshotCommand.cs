namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightGetScreenshotCommand : AutomatedAppCommand<PlaywrightAutomatedApp>
{
	public PlaywrightGetScreenshotCommand()
		: base(CommonCommandNames.GetScreenshot)
	{
	}

	public override object? Execute(PlaywrightAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (app.Page.ScreenshotAsync().GetAwaiter().GetResult() is { } screenshot)
			return new ByteArrayInMemoryFile(screenshot);

		return null;
	}
}
