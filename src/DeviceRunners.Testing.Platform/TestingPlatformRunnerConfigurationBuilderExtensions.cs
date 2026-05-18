namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Extension methods for <see cref="ITestingPlatformRunnerConfigurationBuilder"/> to configure
/// streaming consumers and CLI configuration.
/// </summary>
public static class TestingPlatformRunnerConfigurationBuilderExtensions
{
	/// <summary>
	/// Adds TCP streaming to send test events to the host tool (native platforms).
	/// Creates the consumer eagerly — MTP's IServiceProvider is separate from the app's DI.
	/// </summary>
	public static TBuilder AddTcpStreaming<TBuilder>(
		this TBuilder builder, TcpStreamingConsumerOptions options)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
	{
		var consumer = new TcpStreamingConsumer(options);
		builder.AddBuilderConfiguration(mtpBuilder =>
		{
			mtpBuilder.TestHost.AddDataConsumer(_ => consumer);
		});
		return builder;
	}

	/// <summary>
	/// Adds console streaming for WASM/Blazor. Events are written to console.log
	/// and captured by the CLI via Chrome DevTools Protocol.
	/// </summary>
	public static TBuilder AddConsoleStreaming<TBuilder>(this TBuilder builder)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
	{
		var consumer = new ConsoleStreamingConsumer();
		builder.AddBuilderConfiguration(mtpBuilder =>
		{
			mtpBuilder.TestHost.AddDataConsumer(_ => consumer);
		});
		return builder;
	}

	/// <summary>
	/// Reads config from environment variables or CLI args (native platforms: Android, iOS, macOS, Windows).
	/// Detects DEVICE_RUNNERS_AUTORUN and configures TCP streaming accordingly.
	/// No-op when DEVICE_RUNNERS_AUTORUN is not set.
	/// </summary>
	public static TBuilder AddCliConfiguration<TBuilder>(this TBuilder builder)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
	{
		var autorun = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_AUTORUN");
		var portStr = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_PORT");
		var hostNamesRaw = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_HOST_NAMES");

		// Fall back to CLI args (Windows MSIX can't use env vars easily)
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
			? new[] { "localhost", "10.0.2.2" }
			: hostNamesRaw.Split(';', StringSplitOptions.RemoveEmptyEntries);

		builder.AddTcpStreaming(new TcpStreamingConsumerOptions
		{
			HostNames = hostNames,
			Port = port,
		});

		return builder;
	}

	/// <summary>
	/// Reads config from URL query parameters (WASM/Blazor).
	/// Detects ?device-runners-autorun=1 and adds ConsoleStreamingConsumer.
	/// Mirrors existing Blazor visual runner AddCliConfiguration(currentUrl) pattern.
	/// </summary>
	public static TBuilder AddCliConfiguration<TBuilder>(this TBuilder builder, string currentUrl)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
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

		builder.AddConsoleStreaming();
		return builder;
	}
}
