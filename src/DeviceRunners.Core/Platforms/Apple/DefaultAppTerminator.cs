namespace DeviceRunners.Core;

public class DefaultAppTerminator : IAppTerminator
{
	public void Terminate()
	{
		var s = new ObjCRuntime.Selector("terminateWithSuccess");
		var app = UIApplication.SharedApplication;
		app.PerformSelector(s, app, 0);
	}
}
