using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Minimal ITestFrameworkCapabilities implementation for DeviceXunitTestFramework.
/// </summary>
sealed class DeviceXunitTestFrameworkCapabilities : ITestFrameworkCapabilities
{
	public IReadOnlyCollection<ITestFrameworkCapability> Capabilities => [];
}
