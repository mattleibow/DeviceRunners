using Xunit.Runner.Devices.XHarness.Maui.Pages;

namespace Xunit.Runner.Devices.XHarness.Maui;

public static class AppHostBuilderExtensions
{
	public static MauiAppBuilder ConfigureXHarness(this MauiAppBuilder appHostBuilder)
	{
		// // register runner components
		// appHostBuilder.Services.AddSingleton(options);
		// appHostBuilder.Services.AddSingleton<ITestRunner, DeviceRunner>();
		// appHostBuilder.Services.AddSingleton<IDiagnosticsManager, DiagnosticsManager>();

		// only register the "root" view models and the others are created by the ITestRunner
		appHostBuilder.Services.AddSingleton<HomeViewModel>();
		// appHostBuilder.Services.AddSingleton<DiagnosticsViewModel>();
		// appHostBuilder.Services.AddSingleton<CreditsViewModel>();

		// register app components
		appHostBuilder.Services.AddSingleton<XHarnessApp>();
		appHostBuilder.Services.AddSingleton<XHarnessWindow>();
		appHostBuilder.Services.AddSingleton<XHarnessAppShell>();

		// register pages
		appHostBuilder.Services.AddTransient<HomePage>();

		return appHostBuilder;
	}
}
