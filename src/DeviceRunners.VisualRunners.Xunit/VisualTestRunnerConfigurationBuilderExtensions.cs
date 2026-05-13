using DeviceRunners.VisualRunners.Xunit;

namespace DeviceRunners.VisualRunners;

public static class VisualTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddXunit<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<XunitTestDiscoverer, XunitTestRunner>();
		return builder;
	}

	/// <summary>
	/// Adds the Xunit test discoverer and runner for browser WebAssembly environments.
	/// Uses reflection-based discovery instead of XunitFrontController, which requires
	/// filesystem access unavailable in WASM.
	/// </summary>
	public static TBuilder AddXunitWasm<TBuilder>(this TBuilder builder)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		builder.AddTestPlatform<XunitWasmTestDiscoverer, XunitWasmTestRunner>();
		return builder;
	}
}
