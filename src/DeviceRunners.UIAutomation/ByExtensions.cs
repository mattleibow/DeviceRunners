namespace DeviceRunners.UIAutomation;

public static class ByExtensions
{
	public static void Id(this IBy by, string id) => by.Selector(BySelectors.Id, id);

	public static void AccessibilityId(this IBy by, string id) => by.Selector(BySelectors.AccessibilityId, id);
}
