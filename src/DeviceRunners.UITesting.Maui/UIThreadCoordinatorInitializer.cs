namespace DeviceRunners.UITesting.Maui.Generated;

internal static class MauiUIThreadCoordinatorInitializer
{
	[global::System.Runtime.CompilerServices.ModuleInitializer]
	internal static void Initialize()
	{
		global::DeviceRunners.UITesting.UIThreadCoordinator.Current = new global::DeviceRunners.UITesting.Maui.MauiUIThreadCoordinator();
	}
}
