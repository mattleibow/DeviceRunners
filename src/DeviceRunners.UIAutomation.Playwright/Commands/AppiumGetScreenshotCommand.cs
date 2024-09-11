//namespace DeviceRunners.UIAutomation.Playwright;

//public class PlaywrightGetScreenshotCommand : AutomatedAppCommand<PlaywrightAutomatedApp>
//{
//	public PlaywrightGetScreenshotCommand()
//		: base(CommonCommandNames.GetScreenshot)
//	{
//	}

//	public override object? Execute(PlaywrightAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
//	{
//		if (app.Driver.GetScreenshot() is { } screenshot)
//			return new Base64StringInMemoryFile(screenshot.AsBase64EncodedString);

//		return null;
//	}
//}
