using DeviceRunners.Core;

using Microsoft.DotNet.XHarness.TestRunners.Common;

using DeviceRunners.XHarness.Maui;
using DeviceRunners.XHarness.Maui.Pages;

namespace DeviceRunners.XHarness;

public static class AppHostBuilderExtensions
{
	public static MauiAppBuilder UseXHarnessTestRunner(this MauiAppBuilder appHostBuilder, Action<XHarnessTestRunnerConfigurationBuilder> configurationBuilder)
	{
		var configBuilder = new XHarnessTestRunnerConfigurationBuilder(appHostBuilder);
		configBuilder.UseEnvironmentVariables();
		configurationBuilder?.Invoke(configBuilder);
		var configuration = configBuilder.Build();

		// register runner components
		appHostBuilder.Services.AddSingleton<IXHarnessTestRunnerConfiguration>(configuration);
		appHostBuilder.Services.AddSingleton<ApplicationOptions>(ApplicationOptions.Current);
		appHostBuilder.Services.AddSingleton<IDevice, XHarnessTestDevice>();
		appHostBuilder.Services.AddSingleton<IAppTerminator, DefaultAppTerminator>();

		// only register the "root" view models
		appHostBuilder.Services.AddSingleton<HomeViewModel>();

		// register app components
		appHostBuilder.Services.AddSingleton<XHarnessWindow>();
		appHostBuilder.Services.AddSingleton<XHarnessAppShell>();

		// register pages
		appHostBuilder.Services.AddTransient<HomePage>();

		if (configBuilder.RunnerUsage == XHarnessTestRunnerUsage.Always ||
			(configBuilder.RunnerUsage == XHarnessTestRunnerUsage.Automatic && XHarnessDetector.IsUsingXHarness))
		{
			Console.WriteLine("Registering the XHarness app as the test runner app.");

			appHostBuilder.UseMauiApp<XHarnessApp>();
		}

		return appHostBuilder;
	}

	public static TBuilder SetTestRunnerUsage<TBuilder>(this TBuilder builder, XHarnessTestRunnerUsage usage)
		where TBuilder : XHarnessTestRunnerConfigurationBuilder
	{
		builder.RunnerUsage = usage;
		return builder;
	}
}
