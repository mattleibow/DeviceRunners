using DeviceRunners.Core;
using DeviceRunners.VisualRunners.Blazor;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// Extension methods for registering the Blazor visual test runner.
/// </summary>
public static class AppHostBuilderExtensions
{
	/// <summary>
	/// Registers the Blazor visual test runner services (ViewModels, configuration, result channels, etc.)
	/// into the provided <see cref="WebAssemblyHostBuilder"/>.
	/// </summary>
	public static WebAssemblyHostBuilder UseVisualTestRunner(
		this WebAssemblyHostBuilder builder,
		Action<VisualTestRunnerConfigurationBuilder> configure)
	{
		var configBuilder = new VisualTestRunnerConfigurationBuilder(builder.Services);
		configure(configBuilder);
		var config = configBuilder.Build();

		builder.Services.AddSingleton<IVisualTestRunnerConfiguration>(config);
		builder.Services.AddSingleton<IDiagnosticsManager, DiagnosticsManager>();
		builder.Services.AddSingleton<IAppTerminator, BlazorAppTerminator>();
		builder.Services.AddSingleton<IResultChannelManager, DefaultResultChannelManager>();
		builder.Services.AddSingleton<HomeViewModel>();
		builder.Services.AddSingleton<DiagnosticsViewModel>();

		return builder;
	}

	/// <summary>
	/// Configures the test runner from the DeviceRunners CLI for browser WASM.
	/// When the CLI launches the browser, it navigates to a URL with
	/// <c>?device-runners-autorun=1</c> to trigger headless mode with NDJSON
	/// console output. When the parameter is absent (manual browser open),
	/// this is a no-op and the interactive visual runner is shown.
	/// <para>
	/// Pass the current page URL from <c>builder.HostEnvironment.BaseAddress</c>
	/// combined with JS interop, or use the overload that accepts the URL string.
	/// </para>
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
