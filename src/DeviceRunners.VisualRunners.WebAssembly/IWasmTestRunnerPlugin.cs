using System.Reflection;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.VisualRunners.WebAssembly;

/// <summary>
/// Plugin interface for test framework integration in WASM environments.
/// Each test framework (Xunit v2, Xunit v3, NUnit) implements this to provide
/// test discovery and execution in browser WebAssembly.
/// </summary>
public interface IWasmTestRunnerPlugin
{
	/// <summary>
	/// Discovers and runs all tests in the given assemblies, reporting results
	/// through the provided result channel.
	/// </summary>
	Task<WasmTestRunResult> RunTestsAsync(
		IEnumerable<Assembly> assemblies,
		IResultChannel resultChannel,
		CancellationToken cancellationToken = default);
}
