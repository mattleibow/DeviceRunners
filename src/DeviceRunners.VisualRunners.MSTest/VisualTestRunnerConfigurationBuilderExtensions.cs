using DeviceRunners.VisualRunners.MSTest;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddMSTest<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<MSTestTestDiscoverer, MSTestTestRunner>();
		return builder;
	}
}
