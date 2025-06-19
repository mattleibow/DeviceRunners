using DeviceRunners.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.VisualRunners;

public static class BlazorAppBuilderExtensions
{
	// default to true as it is always supported
	internal static bool IsUsingVisualRunner { get; set; } = true;

	public static WebApplicationBuilder UseVisualTestRunner(this WebApplicationBuilder appBuilder, Action<BlazorVisualTestRunnerConfigurationBuilder> configurationBuilder)
	{
		var configBuilder = new BlazorVisualTestRunnerConfigurationBuilder(appBuilder);
		configurationBuilder?.Invoke(configBuilder);
		var configuration = configBuilder.Build();

		// register runner components
		appBuilder.Services.AddSingleton<IVisualTestRunnerConfiguration>(configuration);
		appBuilder.Services.AddSingleton<IDiagnosticsManager, DiagnosticsManager>();
		appBuilder.Services.AddSingleton<IAppTerminator, DefaultAppTerminator>();
		appBuilder.Services.AddSingleton<IResultChannelManager, DefaultResultChannelManager>();

		// only register the "root" view models and the others are created by the ITestRunner
		appBuilder.Services.AddSingleton<HomeViewModel>();
		appBuilder.Services.AddSingleton<DiagnosticsViewModel>();
		appBuilder.Services.AddSingleton<CreditsViewModel>();

		if (configBuilder.RunnerUsage == VisualTestRunnerUsage.Always ||
			(configBuilder.RunnerUsage == VisualTestRunnerUsage.Automatic && IsUsingVisualRunner))
		{
			Console.WriteLine("Registering the visual runner app as the test runner app.");

			// Add Blazor Server components
			appBuilder.Services.AddRazorComponents()
				.AddInteractiveServerComponents();
		}

		return appBuilder;
	}

	public static TBuilder SetTestRunnerUsage<TBuilder>(this TBuilder builder, VisualTestRunnerUsage usage)
		where TBuilder : BlazorVisualTestRunnerConfigurationBuilder
	{
		builder.RunnerUsage = usage;
		return builder;
	}
}