using DeviceRunners.Core;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Launches MTP test runner after the MAUI app is fully initialized.
/// Only activates when DEVICE_RUNNERS_AUTORUN=1 is set.
/// Uses Task.Run to avoid blocking the MAUI initialization pipeline.
/// </summary>
sealed class MtpTestRunnerService : IMauiInitializeService
{
	readonly ITestingPlatformRunnerConfiguration _configuration;
	readonly IAppTerminator _terminator;

	public MtpTestRunnerService(
		ITestingPlatformRunnerConfiguration configuration,
		IAppTerminator terminator)
	{
		_configuration = configuration;
		_terminator = terminator;
	}

	public void Initialize(IServiceProvider services)
	{
		if (Environment.GetEnvironmentVariable("DEVICE_RUNNERS_AUTORUN") != "1")
			return;

		_ = Task.Run(async () =>
		{
			// Small delay to allow the platform UI to become responsive.
			await Task.Delay(500);

			try
			{
				var args = BuildMtpArgs();
				Action<Microsoft.Testing.Platform.Builder.ITestApplicationBuilder> compositeConfig = builder =>
				{
					foreach (var config in _configuration.BuilderConfigurations)
						config(builder);
				};

				await _configuration.TestFrameworkFactory!(args, compositeConfig);
			}
			finally
			{
				_terminator.Terminate();
			}
		});
	}

	static string[] BuildMtpArgs()
	{
		var args = new List<string>();
		var filter = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_FILTER");
		if (!string.IsNullOrEmpty(filter))
			args.AddRange(["--treenode-filter", filter]);
		return [.. args];
	}
}
