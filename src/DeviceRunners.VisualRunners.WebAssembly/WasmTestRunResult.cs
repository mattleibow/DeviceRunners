namespace DeviceRunners.VisualRunners.WebAssembly;

/// <summary>
/// Result of a WASM test run, providing summary counts and exit code.
/// </summary>
public class WasmTestRunResult
{
	public int TotalTests { get; init; }

	public int PassedTests { get; init; }

	public int FailedTests { get; init; }

	public int SkippedTests { get; init; }

	public int ExitCode => FailedTests > 0 ? 1 : 0;
}
