using DeviceRunners.Core;

using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.VisualRunners.Blazor;

/// <summary>
/// Extension methods for registering the Blazor visual test runner in a DI container.
/// </summary>
public static class BlazorVisualRunnerExtensions
{
	/// <summary>
	/// Registers the Blazor visual test runner services (ViewModels, configuration, result channels, etc.)
	/// into the provided <see cref="IServiceCollection"/>.
	/// </summary>
	public static IServiceCollection AddBlazorVisualTestRunner(
		this IServiceCollection services,
		Action<BlazorVisualTestRunnerConfigurationBuilder> configure)
	{
		var builder = new BlazorVisualTestRunnerConfigurationBuilder(services);
		configure(builder);
		var config = builder.Build();

		services.AddSingleton<IVisualTestRunnerConfiguration>(config);
		services.AddSingleton<IDiagnosticsManager, DiagnosticsManager>();
		services.AddSingleton<IAppTerminator, BlazorAppTerminator>();
		services.AddSingleton<IResultChannelManager, DefaultResultChannelManager>();
		services.AddSingleton<HomeViewModel>();
		services.AddSingleton<DiagnosticsViewModel>();

		return services;
	}
}
