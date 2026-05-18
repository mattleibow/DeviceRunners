using Foundation;

namespace DeviceTestingKitApp.MtpDeviceTests;

[Register(nameof(AppDelegate))]
partial class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
