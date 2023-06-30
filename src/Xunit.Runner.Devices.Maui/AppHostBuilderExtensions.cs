using Xunit.Runner.Devices.Maui.Pages;

namespace Xunit.Runner.Devices.Maui
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureRunner(this MauiAppBuilder appHostBuilder, RunnerOptions options)
		{
			// register runner components
			appHostBuilder.Services.AddSingleton(options);
			appHostBuilder.Services.AddSingleton<ITestRunner, DeviceRunner>();

			// only register the "root" view models and the others are created by the ITestRunner
			appHostBuilder.Services.AddTransient<HomeViewModel>();
			appHostBuilder.Services.AddTransient<CreditsViewModel>();

			// register app components
			appHostBuilder.Services.AddSingleton<TestRunnerApp>();
			appHostBuilder.Services.AddSingleton<TestRunnerWindow>();
			appHostBuilder.Services.AddSingleton<TestRunnerAppShell>();

			// register pages
			appHostBuilder.Services.AddTransient<HomePage>();
			appHostBuilder.Services.AddTransient<TestAssemblyPage>();
			appHostBuilder.Services.AddTransient<TestResultPage>();
			appHostBuilder.Services.AddTransient<CreditsPage>();

			return appHostBuilder;
		}
	}
}
