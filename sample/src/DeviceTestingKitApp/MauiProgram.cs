using Microsoft.Extensions.Logging;

namespace DeviceTestingKitApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.AddDeviceTestingKitAppServices();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
