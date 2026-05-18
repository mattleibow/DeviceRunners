using Microsoft.Extensions.DependencyInjection;

using DeviceRunners.Core;

namespace DeviceRunners.Testing.Platform;

public static class AppHostBuilderExtensions
{
	/// <summary>
	/// Registers the MTP test runner with the MAUI app.
	/// When DEVICE_RUNNERS_AUTORUN=1, tests auto-run on app launch.
	/// When not set, the app runs normally with no side effects.
	/// </summary>
	public static MauiAppBuilder UseTestingPlatformRunner(
		this MauiAppBuilder appHostBuilder,
		Action<TestingPlatformRunnerConfigurationBuilder> configure)
	{
		var configBuilder = new TestingPlatformRunnerConfigurationBuilder(appHostBuilder);
		configure(configBuilder);
		var configuration = configBuilder.Build();

		appHostBuilder.Services.AddSingleton<ITestingPlatformRunnerConfiguration>(configuration);
		appHostBuilder.Services.AddSingleton<IAppTerminator, DefaultAppTerminator>();
		appHostBuilder.Services.AddSingleton<IMauiInitializeService, MtpTestRunnerService>();

		return appHostBuilder;
	}
}
