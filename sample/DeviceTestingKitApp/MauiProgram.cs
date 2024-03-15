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
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// app pages
		builder.Services.AddTransient<MainPage>();

		// maui class library
		builder.Services.AddTransient<ISemanticAnnouncer, MauiSemanticAnnouncer>();
		builder.Services.AddTransient<ISemanticScreenReader>(_ => SemanticScreenReader.Default);

		// plain class library
		builder.Services.AddTransient<CounterViewModel>();

		return builder.Build();
	}
}
