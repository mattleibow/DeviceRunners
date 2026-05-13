using DeviceRunners.VisualRunners.Xunit;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddXunit<TBuilder>(this TBuilder builder, bool useReflection = false)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		if (useReflection)
			builder.AddTestPlatform<XunitReflectionTestDiscoverer, XunitReflectionTestRunner>();
		else
			builder.AddTestPlatform<XunitTestDiscoverer, XunitTestRunner>();
		return builder;
	}
}
