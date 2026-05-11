using DeviceRunners.VisualRunners.WebAssembly;

namespace DeviceRunners.VisualRunners.Xunit;

public static class XunitWasmTestRunnerBuilderExtensions
{
	/// <summary>
	/// Configures the WASM test runner to use Xunit v2 for test discovery and execution.
	/// </summary>
	public static WasmTestRunnerBuilder AddXunit(this WasmTestRunnerBuilder builder)
	{
		builder.UsePlugin(new XunitWasmTestRunnerPlugin());
		return builder;
	}
}
