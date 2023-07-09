namespace CommunityToolkit.DeviceRunners.Xunit.Maui;

public static class AppHostBuilderExtensions
{
	public static MauiAppBuilder ConfigureTestRunners(this MauiAppBuilder appHostBuilder, RunnerOptions options)
	{
		// register runner components
		appHostBuilder.Services.AddSingleton(options);

		return appHostBuilder;
	}
}
