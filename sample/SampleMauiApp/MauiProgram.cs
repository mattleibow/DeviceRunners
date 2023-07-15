using Microsoft.Extensions.Logging;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.XHarness;

namespace SampleMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseXHarnessTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(SampleXunitTestProject.UnitTests).Assembly)
				.AddXunit())
			.UseVisualTestRunner(conf => conf
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
