using Microsoft.Extensions.Logging;

using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;

namespace SampleMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.ConfigureUITesting()
			.UseXHarnessTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(SampleXunitTestProject.UnitTests).Assembly)
				.AddXunit())
			.UseVisualTestRunner(conf => conf
#if NON_INTERACTIVE
				.EnableAutoStart(true)
#endif
				.AddResultChannel(new TcpResultChannel(["localhost", "10.0.2.2"], 16384, new TextResultChannelFormatter(), false))
				.AddConsoleResultChannel()
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(SampleXunitTestProject.UnitTests).Assembly)
				.AddTestAssemblies(typeof(SampleNUnitTestProject.UnitTests).Assembly)
				.AddXunit()
				.AddNUnit());

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
