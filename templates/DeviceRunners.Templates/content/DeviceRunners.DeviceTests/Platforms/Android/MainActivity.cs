using Android.App;
using Android.Content.PM;

namespace DeviceRunners.DeviceTests;

[Activity(
	Name = "com.companyname.devicerunners.devicetests.MainActivity",
	Theme = "@style/Maui.SplashTheme",
	MainLauncher = true,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
