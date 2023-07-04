using Microsoft.DotNet.XHarness.TestRunners.Common;

using Xunit.Runner.Devices.Maui;
using Xunit.Runner.Devices.XHarness.Maui.Pages;

namespace Xunit.Runner.Devices.XHarness.Maui;

public static class AppHostBuilderExtensions
{
	internal static bool IsUsingXHarness { get; set; }

	static AppHostBuilderExtensions()
	{
		// This is mostly for iOS and Mac Catalyst as XHarness sets these variables to indicate this is a run
		// that will start the app, run the tests and then exit. If this is the case, then we can use XHarness.
		// For Android, these variables are not set, but our entry point is in the instrumentation.
		if (Environment.GetEnvironmentVariable("NUNIT_AUTOEXIT")?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) == true &&
			Environment.GetEnvironmentVariable("NUNIT_ENABLE_XML_OUTPUT")?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) == true)
		{
			IsUsingXHarness = true;

			Console.WriteLine("Detected that the XHarness variables for auto-exit and xml output were set, so will request an XHarness test runner.");
		}
	}

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

		if (usage == TestRunnerUsage.Always || (usage == TestRunnerUsage.Automatic && IsUsingXHarness))
			appHostBuilder.UseMauiApp<XHarnessApp>();

		return appHostBuilder;
	}
}
