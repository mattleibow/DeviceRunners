using Microsoft.Extensions.Logging;
using CommunityToolkit.DeviceRunners.VisualRunners;
using CommunityToolkit.DeviceRunners.VisualRunners.Maui;

namespace SampleMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseVisualTestRunner(builder => builder
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddTestAssemblies(typeof(XunitTestProject.UnitTests).Assembly)
				.AddXunit());

			// .UseXHarnessTestRunner(options => options
			// 	.AddTestAssembly()
			// 	.AddTestAssemblies()
			// 	.AddXunit());

			// .ConfigureTestRunners(new RunnerOptions
			// {
			// 	Assemblies =
			// 	{
			// 		typeof(MauiProgram).Assembly,
			// 		typeof(UnitTest1).Assembly,
			// 	},
			// })
			// .ConfigureXHarnessTestRunner()
			// .ConfigureVisualTestRunner();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
