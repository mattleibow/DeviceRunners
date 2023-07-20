using System.Diagnostics;

using Microsoft.DotNet.XHarness.Common;
using Microsoft.DotNet.XHarness.iOS.Shared.Execution;
using Microsoft.DotNet.XHarness.TestRunners.Common;

using Mono.Options;

namespace DeviceRunners.XHarness;

public static class XHarnessDetector
{
	public static bool IsUsingXHarness { get; internal set; }

	static XHarnessDetector()
	{
		var opts = ApplicationOptions.Current;
		var add = AdditionalApplicationOptions.Current;

		// This is mostly for iOS and Mac Catalyst as XHarness sets these variables to indicate this is a run
		// that will start the app, run the tests and then exit. If this is the case, then we can use XHarness.
		// For Android, these variables are not set, but our entry point is in the instrumentation.
		if (opts.TerminateAfterExecution && opts.EnableXml)
		{
			IsUsingXHarness = true;

			Console.WriteLine("Detected that the XHarness variables for auto-exit and xml output were set, so will request an XHarness test runner.");
		}
		else if (add.UseXHarness)
		{
			IsUsingXHarness = true;

			Console.WriteLine("Detected that the XHarness command line argument was specified, so will request an XHarness test runner.");
		}
	}
}
