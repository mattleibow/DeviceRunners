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
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register the same services the real app uses
		builder.Services.AddTransient<DeviceTestingKitApp.ViewModels.CounterViewModel>();
		builder.Services.AddTransient<DeviceTestingKitApp.ViewModels.MainViewModel>();

		return builder.Build();
	}
}
