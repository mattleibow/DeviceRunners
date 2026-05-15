using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
		builder.AddResultChannel(_ => new ConsoleResultChannel());
#if WINDOWS
		builder.AddResultChannel(_ => new DebugResultChannel());
#endif
		return builder;
	}

	public static TBuilder AddTcpResultChannel<TBuilder>(this TBuilder builder, TcpResultChannelOptions options)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddResultChannel(svc => new TcpResultChannel(options, svc.GetService<ILoggerFactory>()?.CreateLogger<TcpResultChannel>()));
		return builder;
	}

	public static TBuilder AddFileResultChannel<TBuilder>(this TBuilder builder, FileResultChannelOptions options)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddResultChannel(_ => new FileResultChannel(options));
		return builder;
	}

	public static TBuilder AddResultChannel<TBuilder, TChannel>(this TBuilder builder, Func<IServiceProvider, TChannel> creator)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
		where TChannel : class, IResultChannel
	{
		builder.AddResultChannel(creator);
		return builder;
	}

	public static TBuilder AddResultChannel<TBuilder>(this TBuilder builder, Func<IServiceProvider, IResultChannel> creator)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddResultChannel<IResultChannel>(svc => creator(svc));
		return builder;
	}
}
