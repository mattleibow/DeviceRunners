using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.Testing.Platform;

public static class AppHostBuilderExtensions
{
	/// <summary>
	/// Registers the MTP test runner with the Blazor WebAssembly app.
	/// When ?device-runners-autorun=1 is in the URL, tests auto-run on app start.
	/// When not set, the app runs normally with no side effects.
	/// </summary>
	public static WebAssemblyHostBuilder UseTestingPlatformRunner(
		this WebAssemblyHostBuilder hostBuilder,
		Action<TestingPlatformRunnerConfigurationBuilder> configure)
	{
		var configBuilder = new TestingPlatformRunnerConfigurationBuilder(hostBuilder.Services);
		configure(configBuilder);
		var configuration = configBuilder.Build();

		hostBuilder.Services.AddSingleton<ITestingPlatformRunnerConfiguration>(configuration);
		hostBuilder.Services.AddSingleton<BlazorMtpTestRunnerService>();

		return hostBuilder;
	}
}
