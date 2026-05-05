using Microsoft.Extensions.Logging;

using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
#if MODE_XHARNESS
using DeviceRunners.XHarness;
#endif

namespace DeviceTestingKitApp.DeviceTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Console.WriteLine("Creating the test runner application:");
		Console.WriteLine(" - Visual test runner");
#if MODE_XHARNESS
		Console.WriteLine(" - XHarness test runner");
#endif

		var builder = MauiApp.CreateBuilder();
		builder
			.ConfigureUITesting()
#if MODE_XHARNESS
			.UseXHarnessTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(DeviceTestingKitApp.MauiLibrary.XunitTests.UnitTests).Assembly)
				.AddXunit())
#endif
			.UseVisualTestRunner(conf => conf
				.UseTestRunnerEnvironment()
				.AddConsoleResultChannel()
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(DeviceTestingKitApp.MauiLibrary.XunitTests.UnitTests).Assembly)
				.AddTestAssemblies(typeof(DeviceTestingKitApp.Library.NUnitTests.UnitTests).Assembly)
				.AddXunit()
				.AddNUnit());

#if DEBUG
		builder.Logging.AddDebug();
#else
		builder.Logging.AddConsole();
#endif

		return builder.Build();
	}
}
