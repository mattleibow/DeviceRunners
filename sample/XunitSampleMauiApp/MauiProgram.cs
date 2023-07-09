using Microsoft.Extensions.Logging;

using XunitTestProject;

using CommunityToolkit.DeviceRunners.Xunit;
using CommunityToolkit.DeviceRunners.Xunit.Maui;
using CommunityToolkit.DeviceRunners.Xunit.VisualRunner;
using CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Maui;
using CommunityToolkit.DeviceRunners.Xunit.XHarness;
using CommunityToolkit.DeviceRunners.Xunit.XHarness.Maui;

namespace XunitSampleMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.ConfigureTestRunners(new RunnerOptions
			{
				Assemblies =
				{
					typeof(MauiProgram).Assembly,
					typeof(UnitTest1).Assembly,
				},
			})
			.ConfigureXHarnessTestRunner()
			.ConfigureVisualTestRunner();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
