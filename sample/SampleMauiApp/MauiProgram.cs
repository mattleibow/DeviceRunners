using Microsoft.Extensions.Logging;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.Maui;
using CommunityToolkit.DeviceRunners.VisualRunners.Xunit;
using CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

namespace SampleMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseVisualTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(XunitTestProject.UnitTests).Assembly)
				.AddTestAssemblies(typeof(NUnitTestProject.UnitTests).Assembly)
				.AddXunit()
				.AddNUnit());

			// .UseXHarnessTestRunner(options => options
			// 	.AddTestAssembly()
			// 	.AddTestAssemblies()
			// 	.AddXunit());

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
