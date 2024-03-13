using System.Reflection;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddTestAssembly<TBuilder>(this TBuilder builder, Assembly assembly)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestAssembly(assembly);
		return builder;
	}

	public static TBuilder AddTestAssemblies<TBuilder>(this TBuilder builder, IEnumerable<Assembly> assemblies)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		foreach (var assembly in assemblies)
			builder.AddTestAssembly(assembly);
		return builder;
	}

	public static TBuilder AddTestAssemblies<TBuilder>(this TBuilder builder, params Assembly[] assemblies)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		foreach (var assembly in assemblies)
			builder.AddTestAssembly(assembly);
		return builder;
	}

	public static TBuilder EnableAutoStart<TBuilder>(this TBuilder builder, bool autoTerminate = false)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.EnableAutoStart(autoTerminate);
		return builder;
	}

	public static TBuilder AddConsoleResultChannel<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddResultChannel(new ConsoleResultChannel());
#if WINDOWS
		builder.AddResultChannel(new DebugResultChannel());
#endif
		return builder;
	}

	public static TBuilder AddResultChannel<TBuilder>(this TBuilder builder, IResultChannel resultChannel)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddResultChannel(resultChannel);
		return builder;
	}
}
