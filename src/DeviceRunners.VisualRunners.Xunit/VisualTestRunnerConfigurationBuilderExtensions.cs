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

	public static TBuilder AddThreadlessXunit<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<ThreadlessXunitTestDiscoverer, ThreadlessXunitTestRunner>();
		return builder;
	}
}
