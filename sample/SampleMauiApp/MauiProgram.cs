using Microsoft.Extensions.Logging;

using TestProject;

using Xunit.Runner.Devices;
using Xunit.Runner.Devices.Maui;

namespace SampleMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<TestRunnerApp>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureRunner(new RunnerOptions
			{
				Assemblies =
				{
					typeof(MauiProgram).Assembly,
					typeof(UnitTest1).Assembly,
				},
				AutoStart = true,
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
