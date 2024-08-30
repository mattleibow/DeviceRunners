namespace DeviceRunners.UIAutomation;

public static class AutomatedAppElementExtensions
{
	public static string? GetText(this IAutomatedAppElement element) =>
		element.ExecuteCommand(CommonCommandNames.GetElementText) as string;

	public static void Click(this IAutomatedAppElement element) =>
		element.ExecuteCommand(CommonCommandNames.ClickElement);

	private static object? ExecuteCommand(this IAutomatedAppElement element, string commandName) =>
		element.App.Commands.Execute(commandName, ("element", element));
}
