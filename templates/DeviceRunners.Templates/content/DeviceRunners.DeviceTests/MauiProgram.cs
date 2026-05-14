using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;

using Microsoft.Extensions.Logging;

namespace DeviceRunners.DeviceTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.ConfigureUITesting()
			.UseVisualTestRunner(conf => conf
				.AddEnvironmentVariables()
				.AddConsoleResultChannel()
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddXunit());

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
