using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace DeviceRunners.Cli.Mtp;

/// <summary>
/// Entry point for MTP host mode. When dotnet test launches the CLI with --server,
/// this creates the MTP TestApplication and registers our DeviceTestFramework.
/// </summary>
static class MtpHost
{
	public static async Task<int> RunAsync(string[] args)
	{
		var builder = await TestApplication.CreateBuilderAsync(args);

		builder.RegisterTestFramework(
			_ => new DeviceTestFrameworkCapabilities(),
			(_, serviceProvider) => new DeviceTestFramework(serviceProvider, args));

		using var app = await builder.BuildAsync();
		return await app.RunAsync();
	}
}
