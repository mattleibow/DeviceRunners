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

	/// <summary>
	/// Configures the test runner based on environment variables set by the
	/// DeviceRunners.Testing.Targets NuGet package. When <c>DEVICE_RUNNERS_AUTORUN=1</c>
	/// is set the runner enables auto-start and connects back to the host via TCP so
	/// results can be collected by the CLI tool.
	/// </summary>
	/// <remarks>
	/// Supported platforms: Android, iOS, macOS (Catalyst), Windows.
	/// <para>
	/// On Android and iOS the variables are baked into the app bundle at build time.
	/// On macOS and Windows the CLI injects them when launching the app process.
	/// </para>
	/// Environment variables read:
	/// <list type="bullet">
	/// <item><term>DEVICE_RUNNERS_AUTORUN</term><description>Set to any non-empty value to enable headless mode.</description></item>
	/// <item><term>DEVICE_RUNNERS_PORT</term><description>TCP port to connect to on the host. Defaults to 16384.</description></item>
	/// <item><term>DEVICE_RUNNERS_HOST_NAMES</term><description>Semicolon-separated list of host names or IPs to try. Defaults to <c>localhost;10.0.2.2</c>.</description></item>
	/// </list>
	/// </remarks>
	public static TBuilder UseTestRunnerEnvironment<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		var autorun = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_AUTORUN");
		if (string.IsNullOrEmpty(autorun))
			return builder;

		var port = int.TryParse(Environment.GetEnvironmentVariable("DEVICE_RUNNERS_PORT"), out var p) ? p : 16384;
		var hostNamesRaw = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_HOST_NAMES");
		var hostNames = string.IsNullOrEmpty(hostNamesRaw)
			? ["localhost", "10.0.2.2"]
			: hostNamesRaw.Split(';', StringSplitOptions.RemoveEmptyEntries);

		builder.EnableAutoStart(autoTerminate: true);
		builder.AddTcpResultChannel(new TcpResultChannelOptions
		{
			HostNames = hostNames,
			Port = port,
			Formatter = new EventStreamFormatter(),
			Required = false,
			Retries = 3,
			RetryTimeout = TimeSpan.FromSeconds(5),
			Timeout = TimeSpan.FromSeconds(30),
		});
		return builder;
	}
}
