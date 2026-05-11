using System.Reflection;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.VisualRunners.WebAssembly;

/// <summary>
/// Builder for configuring and running tests in a browser WASM environment.
/// This is the non-MAUI entry point — analogous to MauiAppBuilder.UseVisualTestRunner()
/// but for standalone WASM test apps.
/// </summary>
public class WasmTestRunnerBuilder
{
	readonly List<Assembly> _assemblies = [];
	IResultChannel? _resultChannel;
	IWasmTestRunnerPlugin? _plugin;

	/// <summary>
	/// Adds a test assembly to be discovered and executed.
	/// </summary>
	public WasmTestRunnerBuilder AddAssembly(Assembly assembly)
	{
		_assemblies.Add(assembly);
		return this;
	}

	/// <summary>
	/// Adds multiple test assemblies to be discovered and executed.
	/// </summary>
	public WasmTestRunnerBuilder AddAssemblies(IEnumerable<Assembly> assemblies)
	{
		_assemblies.AddRange(assemblies);
		return this;
	}

	/// <summary>
	/// Sets the result channel for reporting test results.
	/// If not set, defaults to <see cref="ConsoleResultChannel"/> with NDJSON format.
	/// </summary>
	public WasmTestRunnerBuilder UseResultChannel(IResultChannel channel)
	{
		_resultChannel = channel;
		return this;
	}

	/// <summary>
	/// Sets the test framework plugin (Xunit, NUnit, etc.) that provides
	/// test discovery and execution.
	/// </summary>
	public WasmTestRunnerBuilder UsePlugin(IWasmTestRunnerPlugin plugin)
	{
		_plugin = plugin;
		return this;
	}

	/// <summary>
	/// Builds and returns a <see cref="WasmTestRunner"/> configured with the specified options.
	/// </summary>
	public WasmTestRunner Build()
	{
		if (_plugin is null)
			throw new InvalidOperationException("A test runner plugin must be configured. Call UsePlugin() with an IWasmTestRunnerPlugin implementation.");

		if (_assemblies.Count == 0)
			throw new InvalidOperationException("At least one test assembly must be added. Call AddAssembly() with a test assembly.");

		var channel = _resultChannel ?? new ConsoleResultChannel();

		return new WasmTestRunner(_assemblies, _plugin, channel);
	}
}
