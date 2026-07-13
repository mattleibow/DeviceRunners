using DeviceRunners.VisualRunners.MSTest3;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddMSTest3<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<MSTest3TestDiscoverer, MSTest3TestRunner>();
		return builder;
	}
}
