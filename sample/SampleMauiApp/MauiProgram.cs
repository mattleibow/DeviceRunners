using Microsoft.Extensions.Logging;

using TestProject;

using Xunit.Runner.Devices;
using Xunit.Runner.Devices.Maui;
using Xunit.Runner.Devices.VisualRunner;
using Xunit.Runner.Devices.VisualRunner.Maui;
using Xunit.Runner.Devices.XHarness;
using Xunit.Runner.Devices.XHarness.Maui;

namespace SampleMauiApp;

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
