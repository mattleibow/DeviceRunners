using DeviceTestingKitApp.Features;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp;

/// <summary>
/// Shared service registration that both the real app and test projects can call.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers all app services, view models, and pages.
	/// Call this from both MauiProgram.cs and from test projects that
	/// reference the app as a library.
	/// </summary>
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
