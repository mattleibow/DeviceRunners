namespace DeviceRunners.UIAutomation.Appium;

public static class AutomatedAppExtensions
{
	public static string? GetPageSource(this IAutomatedApp app) =>
		app.Commands.Execute(AppiumCommandNames.GetPageSource) as string;

	public static IInMemoryFile? GetScreenshot(this IAutomatedApp app) =>
		app.Commands.Execute(AppiumCommandNames.GetScreenshot) as IInMemoryFile;

	public static void DismissKeyboard(this IAutomatedApp app) =>
		app.Commands.Execute(AppiumCommandNames.DismissKeyboard);
}
