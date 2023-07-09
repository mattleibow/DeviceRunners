using Android.App;
using Android.OS;
using Android.Runtime;

namespace CommunityToolkit.DeviceRunners.Xunit.XHarness.Maui;

[Instrumentation(Name = "communitytoolkit.devicerunners.xunit.xharness.maui.XHarnessInstrumentation")]
public class XHarnessInstrumentation : XHarnessInstrumentationBase
{
	protected XHarnessInstrumentation(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate(Bundle? arguments)
	{
		// This will mark the XHarness runner as an option since we are coming in directly from an instrumentation.
		// For iOS or Mac Catalyst, we use environment variables.
		AppHostBuilderExtensions.IsUsingXHarness = true;

		Console.WriteLine("Detected that the entry point was through the XHarness instrumentation, so will request an XHarness test runner.");

		base.OnCreate(arguments);
	}

	protected override HomeViewModel GetHomeViewModel(Android.App.Application app)
	{
		if (app is not MauiApplication mauiApp)
			throw new InvalidOperationException("The .NET MAUI instrumentation implementation only supports MauiApplication.");

		var services = mauiApp.Services;
		return services.GetRequiredService<HomeViewModel>();
	}
}
