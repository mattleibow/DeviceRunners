using Microsoft.DotNet.XHarness.TestRunners.Common;

using Xunit.Runner.Devices.Maui;
using Xunit.Runner.Devices.XHarness.Maui.Pages;

namespace Xunit.Runner.Devices.XHarness.Maui;

public static class AppHostBuilderExtensions
{
	public static MauiAppBuilder ConfigureXHarnessTestRunner(this MauiAppBuilder appHostBuilder, TestRunnerUsage usage = TestRunnerUsage.Automatic)
	{
		// register runner components
		appHostBuilder.Services.AddSingleton<IDevice, XHarnessTestDevice>();
		appHostBuilder.Services.AddSingleton<ApplicationOptions>(ApplicationOptions.Current);

#if IOS || MACCATALYST || ANDROID
		appHostBuilder.Services.AddSingleton<ITestRunner, XHarnessTestRunner>();
#endif

		// only register the "root" view models
		appHostBuilder.Services.AddSingleton<HomeViewModel>();

		// register app components
		appHostBuilder.Services.AddSingleton<XHarnessApp>();
		appHostBuilder.Services.AddSingleton<XHarnessWindow>();
		appHostBuilder.Services.AddSingleton<XHarnessAppShell>();

		// register pages
		appHostBuilder.Services.AddTransient<HomePage>();

		if (usage == TestRunnerUsage.Always || (usage == TestRunnerUsage.Automatic))
			appHostBuilder.UseMauiApp<XHarnessApp>();

		return appHostBuilder;
	}
}
