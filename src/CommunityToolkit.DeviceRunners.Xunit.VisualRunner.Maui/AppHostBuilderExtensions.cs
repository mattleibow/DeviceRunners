using CommunityToolkit.DeviceRunners.Xunit.Maui;
using CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Maui.Pages;

namespace CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Maui;

public static class AppHostBuilderExtensions
{
	// default to true as it is always supported
	internal static bool IsUsingVisualRunner { get; set; } = true;

	public static MauiAppBuilder ConfigureVisualTestRunner(this MauiAppBuilder appHostBuilder, TestRunnerUsage usage = TestRunnerUsage.Automatic)
	{
		// register runner components
		appHostBuilder.Services.AddSingleton<ITestRunner, VisualTestRunner>();
		appHostBuilder.Services.AddSingleton<IDiagnosticsManager, DiagnosticsManager>();

		// only register the "root" view models and the others are created by the ITestRunner
		appHostBuilder.Services.AddSingleton<HomeViewModel>();
		appHostBuilder.Services.AddSingleton<DiagnosticsViewModel>();
		appHostBuilder.Services.AddSingleton<CreditsViewModel>();

		// register app components
		appHostBuilder.Services.AddSingleton<VisualRunnerApp>();
		appHostBuilder.Services.AddSingleton<VisualRunnerWindow>();
		appHostBuilder.Services.AddSingleton<VisualRunnerAppShell>();

		// register pages
		appHostBuilder.Services.AddTransient<HomePage>();
		appHostBuilder.Services.AddTransient<TestAssemblyPage>();
		appHostBuilder.Services.AddTransient<TestResultPage>();
		appHostBuilder.Services.AddTransient<CreditsPage>();
		appHostBuilder.Services.AddTransient<DiagnosticsPage>();

		if (usage == TestRunnerUsage.Always || (usage == TestRunnerUsage.Automatic && IsUsingVisualRunner))
			appHostBuilder.UseMauiApp<VisualRunnerApp>();

		return appHostBuilder;
	}
}
