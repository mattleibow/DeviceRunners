using Android.App;
using Android.Content.PM;
using Android.OS;

namespace DeviceTestingKitApp.AppTests;

[Activity(Name = "com.companyname.devicetestingkitapp.apptests.MainActivity", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
