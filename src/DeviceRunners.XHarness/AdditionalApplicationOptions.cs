using System.Diagnostics;

using Mono.Options;

namespace DeviceRunners.XHarness;

class AdditionalApplicationOptions
{
	public static AdditionalApplicationOptions Current = new();

	public bool UseXHarness { get; private set; }

	public string? OutputDirectory { get; private set; }

	public AdditionalApplicationOptions()
	{
		if (bool.TryParse(Environment.GetEnvironmentVariable("XHARNESS_RUNNER_ENABLED"), out bool b))
			UseXHarness = b;

		var os = new OptionSet
		{
			{ "xharness", "Run using XHarness test runner", v => UseXHarness = true },
			{ "output-directory=", "Output directory for the test results", v => OutputDirectory  = v },
		};

		try
		{
			os.Parse(Environment.GetCommandLineArgs());
		}
		catch (OptionException oe)
		{
			Debug.WriteLine("{0} for options '{1}'", oe.Message, oe.OptionName);
		}
	}
}
