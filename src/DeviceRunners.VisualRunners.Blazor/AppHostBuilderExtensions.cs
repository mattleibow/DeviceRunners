using DeviceRunners.Core;
using DeviceRunners.VisualRunners.Blazor;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// Extension methods for registering the Blazor visual test runner.
/// </summary>
public static class AppHostBuilderExtensions
{
	/// <summary>
	/// Registers the Blazor visual test runner services (ViewModels, configuration, result channels, etc.)
	/// into the provided <see cref="WebAssemblyHostBuilder"/>.
	/// </summary>
	public static WebAssemblyHostBuilder UseVisualTestRunner(
		this WebAssemblyHostBuilder builder,
		Action<VisualTestRunnerConfigurationBuilder> configure)
	{
		var configBuilder = new VisualTestRunnerConfigurationBuilder(builder.Services);
		configure(configBuilder);
		var config = configBuilder.Build();

		builder.Services.AddSingleton<IVisualTestRunnerConfiguration>(config);
		builder.Services.AddSingleton<IDiagnosticsManager, DiagnosticsManager>();
		builder.Services.AddSingleton<IAppTerminator, BlazorAppTerminator>();
		builder.Services.AddSingleton<IResultChannelManager, DefaultResultChannelManager>();
		builder.Services.AddSingleton<HomeViewModel>();
		builder.Services.AddSingleton<DiagnosticsViewModel>();

		return builder;
	}
}
