using Foundation;

namespace DeviceTestingKitApp.AppTests;

[Register(nameof(AppDelegate))]
partial class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
