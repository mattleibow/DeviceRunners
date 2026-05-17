using DeviceRunners.VisualRunners.Xunit3;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddXunit3<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<Xunit3TestDiscoverer, Xunit3TestRunner>();
		return builder;
	}
}
