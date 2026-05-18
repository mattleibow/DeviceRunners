using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace DeviceRunners.Cli.Mtp;

/// <summary>
/// Capabilities for the device test framework host bridge.
/// </summary>
sealed class DeviceTestFrameworkCapabilities : ITestFrameworkCapabilities
{
	public IReadOnlyCollection<ITestFrameworkCapability> Capabilities { get; } = [];
}
