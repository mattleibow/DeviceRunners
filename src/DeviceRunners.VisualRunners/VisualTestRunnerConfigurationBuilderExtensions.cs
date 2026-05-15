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

	/// <summary>
	/// Configures the test runner from the DeviceRunners CLI or <c>dotnet test</c>.
	/// Reads configuration from environment variables first, then falls back
	/// to command-line arguments. This supports all launch mechanisms:
	/// <list type="bullet">
	/// <item>Environment variables: Android (build-time), iOS (SimCtl), macOS/Windows EXE (ProcessStartInfo)</item>
	/// <item>CLI arguments: Windows MSIX (via winapp.exe --args)</item>
	/// </list>
	/// When neither source provides <c>DEVICE_RUNNERS_AUTORUN</c>, this is a no-op
	/// and the visual runner behaves normally (e.g., when launched from the IDE).
	/// </summary>
	public static TBuilder AddCliConfiguration<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		// Try environment variables first (works for all platforms except MSIX)
		var autorun = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_AUTORUN");
		var portStr = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_PORT");
		var hostNamesRaw = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_HOST_NAMES");

		// Fall back to CLI arguments (for MSIX where env vars are not forwarded)
		if (string.IsNullOrEmpty(autorun))
		{
			var args = Environment.GetCommandLineArgs();
			for (var i = 0; i < args.Length; i++)
			{
				if (args[i] == "--device-runners-autorun")
					autorun = "1";
				else if (args[i] == "--device-runners-port" && i + 1 < args.Length)
					portStr = args[++i];
				else if (args[i] == "--device-runners-host-names" && i + 1 < args.Length)
					hostNamesRaw = args[++i];
			}
		}

		if (string.IsNullOrEmpty(autorun))
			return builder;

		var port = int.TryParse(portStr, out var p) ? p : 16384;
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

	/// <summary>
	/// Configures the test runner from a URL containing query parameters.
	/// Parses the provided URL for <c>device-runners-autorun</c> query parameter.
	/// When the CLI launches the browser, it navigates to a URL with
	/// <c>?device-runners-autorun=1</c> to trigger headless mode with NDJSON
	/// console output. When the parameter is absent (manual browser open),
	/// this is a no-op and the interactive visual runner is shown.
	/// </summary>
	public static TBuilder AddCliConfiguration<TBuilder>(this TBuilder builder, string currentUrl)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		string? autorun = null;

		try
		{
			var qIdx = currentUrl.IndexOf('?');
			if (qIdx >= 0)
			{
				var query = currentUrl[(qIdx + 1)..];
				foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
				{
					var eqIdx = pair.IndexOf('=');
					var key = eqIdx >= 0 ? Uri.UnescapeDataString(pair[..eqIdx]) : pair;
					var value = eqIdx >= 0 ? Uri.UnescapeDataString(pair[(eqIdx + 1)..]) : "1";

					if (key.Equals("device-runners-autorun", StringComparison.OrdinalIgnoreCase))
						autorun = value;
				}
			}
		}
		catch
		{
			// Not a valid URL — ignore
		}

		if (string.IsNullOrEmpty(autorun))
			return builder;

		builder.EnableAutoStart(autoTerminate: true);
		builder.AddResultChannel(_ => new ConsoleResultChannel(new EventStreamFormatter()));
		return builder;
	}
}
