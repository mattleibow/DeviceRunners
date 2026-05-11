using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.WebAssembly;
using DeviceRunners.VisualRunners.Xunit;

// Build the WASM test runner with Xunit plugin and console output
var exitCode = await new WasmTestRunnerBuilder()
	.AddAssembly(typeof(SampleXunitTests).Assembly)
	.AddXunit()
	.UseResultChannel(new ConsoleResultChannel())
	.Build()
	.RunAsync();

return exitCode;
