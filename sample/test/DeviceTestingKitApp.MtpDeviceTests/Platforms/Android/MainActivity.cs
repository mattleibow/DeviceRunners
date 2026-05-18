using Android.App;
using Android.Content.PM;

namespace DeviceTestingKitApp.MtpDeviceTests;

[Activity(Name = "com.companyname.devicetestingkitapp.mtpdevicetests.MainActivity", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
