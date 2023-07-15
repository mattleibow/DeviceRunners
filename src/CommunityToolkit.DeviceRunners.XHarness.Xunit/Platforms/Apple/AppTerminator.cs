using ObjCRuntime;
using UIKit;

namespace CommunityToolkit.DeviceRunners.XHarness.Xunit;

static class AppTerminator
{
	public static void Terminate()
	{
		var s = new ObjCRuntime.Selector("terminateWithSuccess");
		var app = UIApplication.SharedApplication;
		app.PerformSelector(s, app, 0);
	}
}
