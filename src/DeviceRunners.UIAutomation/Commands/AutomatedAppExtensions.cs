namespace DeviceRunners.UIAutomation;

public static class AutomatedAppExtensions
{
	public static string? GetPageSource(this IAutomatedApp app) =>
		app.Commands.Execute(CommonCommandNames.GetPageSource) as string;

	public static IInMemoryFile? GetScreenshot(this IAutomatedApp app) =>
		app.Commands.Execute(CommonCommandNames.GetScreenshot) as IInMemoryFile;

	public static void Click(this IAutomatedApp app, IAutomatedAppElement element) =>
		element.Click();

	public static void Click(this IAutomatedApp app, int x, int y) =>
		app.Commands.Execute(CommonCommandNames.ClickCoordinates, ("x", x), ("y", y));
}
