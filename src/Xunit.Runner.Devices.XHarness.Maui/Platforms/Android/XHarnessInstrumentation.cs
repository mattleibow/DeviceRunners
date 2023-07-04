using Android.App;
using Android.Runtime;

namespace Xunit.Runner.Devices.XHarness.Maui;

[Instrumentation(Name = "xunit.runner.devices.xharness.maui.XHarnessInstrumentation")]
public class XHarnessInstrumentation : XHarnessInstrumentationBase
{
	protected XHarnessInstrumentation(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override HomeViewModel GetHomeViewModel(Android.App.Application app)
	{
		if (app is not MauiApplication mauiApp)
			throw new InvalidOperationException("The .NET MAUI instrumentation implementation only supports MauiApplication.");

		var services = mauiApp.Services;
		return services.GetRequiredService<HomeViewModel>();
	}
}
