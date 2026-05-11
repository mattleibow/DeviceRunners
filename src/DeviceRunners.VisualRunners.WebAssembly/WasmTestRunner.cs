using System.Reflection;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.VisualRunners.WebAssembly;

/// <summary>
/// Orchestrates test execution in a browser WASM environment.
/// Manages the result channel lifecycle and delegates to the configured plugin.
/// </summary>
public class WasmTestRunner
{
	readonly IReadOnlyList<Assembly> _assemblies;
	readonly IWasmTestRunnerPlugin _plugin;
	readonly IResultChannel _resultChannel;

	internal WasmTestRunner(
		IReadOnlyList<Assembly> assemblies,
		IWasmTestRunnerPlugin plugin,
		IResultChannel resultChannel)
	{
		_assemblies = assemblies;
		_plugin = plugin;
		_resultChannel = resultChannel;
	}

	/// <summary>
	/// Runs all tests and returns the exit code (0 = success, 1 = failures).
	/// Opens and closes the result channel automatically.
	/// </summary>
	public async Task<int> RunAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _resultChannel.OpenChannel("WASM Test Run");

			var result = await _plugin.RunTestsAsync(_assemblies, _resultChannel, cancellationToken);

			var summary =
				$"Tests run: {result.TotalTests} " +
				$"Passed: {result.PassedTests} " +
				$"Failed: {result.FailedTests} " +
				$"Skipped: {result.SkippedTests}";

			Console.WriteLine(summary);

			return result.ExitCode;
		}
		finally
		{
			await _resultChannel.CloseChannel();
		}
	}
}
