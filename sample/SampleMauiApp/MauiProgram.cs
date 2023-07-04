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
		Console.WriteLine("YOU ARE HERE: CreateMauiApp");
		var vars = Environment.GetEnvironmentVariables();
		foreach (var key in vars.Keys)
			Console.WriteLine($"  '{key}' = '{vars[key]}'");

		var builder = MauiApp.CreateBuilder();
		builder
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureTestRunners(new RunnerOptions
			{
				Assemblies =
				{
					typeof(MauiProgram).Assembly,
					typeof(UnitTest1).Assembly,
				},
			})
			.ConfigureXHarnessTestRunner(TestRunnerUsage.Never)
			.ConfigureVisualTestRunner(TestRunnerUsage.Always);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
