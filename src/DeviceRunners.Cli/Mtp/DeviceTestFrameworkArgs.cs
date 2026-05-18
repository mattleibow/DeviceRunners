namespace DeviceRunners.Cli.Mtp;

/// <summary>
/// Parsed CLI arguments for the host-side device test framework.
/// These are extracted from the args passed to the CLI binary by MSBuild/dotnet test.
/// </summary>
sealed class DeviceTestFrameworkArgs
{
	public string? Platform { get; init; }
	public string? App { get; init; }
	public string? Device { get; init; }
	public int Port { get; init; } = 16384;
	public int ConnectionTimeout { get; init; } = 120;
	public int DataTimeout { get; init; } = 30;
	public string? HostNames { get; init; }

	/// <summary>
	/// Parses the CLI args that appear before --server (platform-specific).
	/// Format: device-runners --platform android --app path/to/app.apk --port 16384 --server ...
	/// </summary>
	public static DeviceTestFrameworkArgs Parse(string[] args)
	{
		var result = new DeviceTestFrameworkArgs();

		string? platform = null;
		string? app = null;
		string? device = null;
		int port = 16384;
		int connectionTimeout = 120;
		int dataTimeout = 30;
		string? hostNames = null;

		for (int i = 0; i < args.Length; i++)
		{
			// Stop at --server (MTP's args follow)
			if (args[i] == "--server")
				break;

			if (i + 1 < args.Length)
			{
				switch (args[i])
				{
					case "--platform":
						platform = args[++i];
						break;
					case "--app":
						app = args[++i];
						break;
					case "--device":
						device = args[++i];
						break;
					case "--port":
						if (int.TryParse(args[++i], out var p))
							port = p;
						break;
					case "--connection-timeout":
						if (int.TryParse(args[++i], out var ct))
							connectionTimeout = ct;
						break;
					case "--data-timeout":
						if (int.TryParse(args[++i], out var dt))
							dataTimeout = dt;
						break;
					case "--host-names":
						hostNames = args[++i];
						break;
				}
			}
		}

		return new DeviceTestFrameworkArgs
		{
			Platform = platform,
			App = app,
			Device = device,
			Port = port,
			ConnectionTimeout = connectionTimeout,
			DataTimeout = dataTimeout,
			HostNames = hostNames,
		};
	}
}
