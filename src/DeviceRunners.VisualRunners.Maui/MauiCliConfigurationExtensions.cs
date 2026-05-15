using Microsoft.Extensions.Logging;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// MAUI-specific CLI configuration extensions for the visual test runner.
/// </summary>
public static class MauiCliConfigurationExtensions
{
	/// <summary>
	/// Configures the test runner from the DeviceRunners CLI or <c>dotnet test</c>.
	/// Reads configuration from environment variables first, then falls back
	/// to command-line arguments. This supports all native platform launch mechanisms:
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
}
