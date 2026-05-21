using DeviceTestingKitApp.Features;
using DeviceTestingKitApp.ViewModels;

using Microsoft.Extensions.Logging;

namespace DeviceTestingKitApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.AddDeviceTestingKitAppServices();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	public static MauiAppBuilder AddDeviceTestingKitAppServices(this MauiAppBuilder builder)
	{
		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
		});

		// app pages
		builder.Services.AddTransient<MainPage>();

		// maui class library
		builder.Services.AddTransient<ISemanticScreenReader>(_ => SemanticScreenReader.Default);
		builder.Services.AddTransient<ISemanticAnnouncer, MauiSemanticAnnouncer>();

		// plain class library
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<CounterViewModel>();

		return builder;
	}
}
