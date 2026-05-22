namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Starts MTP after the Blazor host builds. On WASM, execution is single-threaded
/// so this runs cooperatively on the main thread.
/// </summary>
public sealed class BlazorMtpTestRunnerService
{
	readonly ITestingPlatformRunnerConfiguration _configuration;

	public BlazorMtpTestRunnerService(ITestingPlatformRunnerConfiguration configuration)
	{
		_configuration = configuration;
	}

	/// <summary>
	/// Starts the MTP test runner. Call this after the WebAssembly host is built.
	/// </summary>
	public async Task StartAsync()
	{
		if (_configuration.TestFrameworkFactory is null)
			return;

		var args = Array.Empty<string>();
		Action<Microsoft.Testing.Platform.Builder.ITestApplicationBuilder> compositeConfig = builder =>
		{
			foreach (var config in _configuration.BuilderConfigurations)
				config(builder);
		};

		await _configuration.TestFrameworkFactory(args, compositeConfig);
	}
}
