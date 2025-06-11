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
#if MODE_NON_INTERACTIVE_VISUAL
		Console.WriteLine(" - Non-interactive visual test runner");
#endif
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
#if MODE_NON_INTERACTIVE_VISUAL
				.EnableAutoStart(true)
				.AddTcpResultChannel(new TcpResultChannelOptions
				{
					HostNames = ["localhost", "10.0.2.2"],
					Port = 16384,
					Formatter = new TextResultChannelFormatter(),
					Required = false
				})
#endif
				.AddConsoleResultChannel()
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(DeviceTestingKitApp.MauiLibrary.XunitTests.UnitTests).Assembly)
				.AddTestAssemblies(typeof(DeviceTestingKitApp.Library.NUnitTests.UnitTests).Assembly)
				.AddXunit()
				.AddNUnit());

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
