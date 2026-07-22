using DeviceRunners.Cli.Commands;

using Xunit;

namespace DeviceRunners.Cli.Tests;

public class WasmTimeoutTests
{
	static WasmTestCommand.Settings NewSettings(int connectionTimeout = 120, int dataTimeout = 30) =>
		new()
		{
			App = "app",
			ConnectionTimeout = connectionTimeout,
			DataTimeout = dataTimeout,
		};

	[Fact]
	public void NoOutputBeforeConnection_ReportsConnectionTimeout()
	{
		var settings = NewSettings(connectionTimeout: 45);

		var reason = WasmTestCommand.DescribeWasmTimeout(settings, anyMessageReceived: false);

		Assert.Contains("connection timeout", reason, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("45", reason);
	}

	[Fact]
	public void SilenceAfterOutput_ReportsDataTimeout()
	{
		var settings = NewSettings(dataTimeout: 15);

		var reason = WasmTestCommand.DescribeWasmTimeout(settings, anyMessageReceived: true);

		Assert.Contains("15", reason);
		Assert.Contains("inactivity", reason, StringComparison.OrdinalIgnoreCase);
	}
}
