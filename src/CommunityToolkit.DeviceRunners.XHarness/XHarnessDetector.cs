namespace CommunityToolkit.DeviceRunners.XHarness;

public static class XHarnessDetector
{
	public static bool IsUsingXHarness { get; internal set; }

	static XHarnessDetector()
	{
		// This is mostly for iOS and Mac Catalyst as XHarness sets these variables to indicate this is a run
		// that will start the app, run the tests and then exit. If this is the case, then we can use XHarness.
		// For Android, these variables are not set, but our entry point is in the instrumentation.
		if (Environment.GetEnvironmentVariable("NUNIT_AUTOEXIT")?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) == true &&
			Environment.GetEnvironmentVariable("NUNIT_ENABLE_XML_OUTPUT")?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) == true)
		{
			IsUsingXHarness = true;

			Console.WriteLine("Detected that the XHarness variables for auto-exit and xml output were set, so will request an XHarness test runner.");
		}
	}
}
