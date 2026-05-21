using DeviceRunners.VisualRunners;

namespace DeviceTestingKitApp.AppTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseVisualTestRunner(conf => conf
				.AddCliConfiguration()
				.AddConsoleResultChannel()
				// Import the real app's styles so {StaticResource} references resolve
				.AddResourceDictionary<DeviceTestingKitApp.Resources.Styles.Colors>()
				.AddResourceDictionary<DeviceTestingKitApp.Resources.Styles.Styles>()
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddNUnit())
			// Register all the real app's services, VMs, pages, and fonts
			.AddDeviceTestingKitAppServices();

		return builder.Build();
	}
}
