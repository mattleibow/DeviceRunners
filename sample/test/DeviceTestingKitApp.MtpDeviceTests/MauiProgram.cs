using DeviceRunners.Testing.Platform;

namespace DeviceTestingKitApp.MtpDeviceTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		builder.UseTestingPlatformRunner(mtp =>
		{
			mtp.AddXunit3();
			mtp.AddCliConfiguration();
		});

		return builder.Build();
	}
}
