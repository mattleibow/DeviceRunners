using DeviceRunners.VisualRunners.WebAssembly;

namespace DeviceRunners.VisualRunners.NUnit;

public static class NUnitWasmTestRunnerBuilderExtensions
{
	/// <summary>
	/// Configures the WASM test runner to use NUnit for test discovery and execution.
	/// </summary>
	public static WasmTestRunnerBuilder AddNUnit(this WasmTestRunnerBuilder builder)
	{
		builder.UsePlugin(new NUnitWasmTestRunnerPlugin());
		return builder;
	}
}
