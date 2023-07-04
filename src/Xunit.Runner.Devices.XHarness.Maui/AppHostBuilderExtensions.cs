using Microsoft.DotNet.XHarness.TestRunners.Common;
using Xunit.Runner.Devices.XHarness.Maui.Pages;

namespace Xunit.Runner.Devices.XHarness.Maui;

public static class AppHostBuilderExtensions
{
	public static MauiAppBuilder ConfigureXHarness(this MauiAppBuilder appHostBuilder, RunnerOptions options)
	{
		// register runner components
		appHostBuilder.Services.AddSingleton(options);
		appHostBuilder.Services.AddSingleton<IDevice, XHarnessTestDevice>();
		appHostBuilder.Services.AddSingleton<ApplicationOptions>(ApplicationOptions.Current);

#if IOS || MACCATALYST
		appHostBuilder.Services.AddSingleton<ITestRunner, XHarnessRunner>();
#endif

		// only register the "root" view models
		appHostBuilder.Services.AddSingleton<HomeViewModel>();

		// register app components
		appHostBuilder.Services.AddSingleton<XHarnessApp>();
		appHostBuilder.Services.AddSingleton<XHarnessWindow>();
		appHostBuilder.Services.AddSingleton<XHarnessAppShell>();

		// register pages
		appHostBuilder.Services.AddTransient<HomePage>();

		return appHostBuilder;
	}
}
