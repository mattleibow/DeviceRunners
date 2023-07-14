using CommunityToolkit.DeviceRunners.XHarness.Xunit;

namespace CommunityToolkit.DeviceRunners.XHarness;

public static class XHarnessTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddXunit<TBuilder>(this TBuilder builder)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<XunitTestRunner>();
		return builder;
	}
}
