using DeviceRunners.XHarness.Xunit;

namespace DeviceRunners.XHarness;

public static class XHarnessTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddXunit<TBuilder>(this TBuilder builder)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
#if ANDROID || IOS || MACCATALYST || WINDOWS
		builder.AddTestPlatform<XunitTestRunner>();
#else
		Console.WriteLine("The XHarness test runner for Xunit is not supported on this platform.");
#endif
		return builder;
	}
}
