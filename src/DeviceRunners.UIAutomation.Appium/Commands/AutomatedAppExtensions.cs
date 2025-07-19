namespace DeviceRunners.UIAutomation.Appium;

public static class AutomatedAppExtensions
{
	public static void DismissKeyboard(this IAutomatedApp app) =>
		app.Commands.Execute(CommonCommandNames.DismissKeyboard);
}
