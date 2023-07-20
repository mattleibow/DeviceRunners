using DeviceRunners.VisualRunners.Xunit;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddXunit<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<XunitTestDiscoverer, XunitTestRunner>();
		return builder;
	}
}
