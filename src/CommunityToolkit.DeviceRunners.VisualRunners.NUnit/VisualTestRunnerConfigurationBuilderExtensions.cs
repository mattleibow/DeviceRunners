using CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

namespace CommunityToolkit.DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddNUnit<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<NUnitTestDiscoverer, NUnitTestRunner>();
		return builder;
	}
}
