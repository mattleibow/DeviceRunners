using Android.App;
using Android.OS;
using Android.Runtime;

namespace DeviceRunners.XHarness.Maui;

[Instrumentation(Name = "devicerunners.xharness.maui.XHarnessInstrumentation")]
public class XHarnessInstrumentation : XHarnessInstrumentationBase
{
	protected XHarnessInstrumentation(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override HomeViewModel GetHomeViewModel(Android.App.Application app)
	{
		if (app is not MauiApplication)
			throw new InvalidOperationException("The .NET MAUI instrumentation implementation only supports MauiApplication.");

		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("Platform application services are not available.");
		return services.GetRequiredService<HomeViewModel>();
	}
}
