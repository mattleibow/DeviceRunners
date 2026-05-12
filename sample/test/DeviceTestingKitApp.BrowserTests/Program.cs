using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.WebAssembly;
using DeviceRunners.VisualRunners.Xunit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var host = builder.Build();

// Run tests after the WASM host starts
_ = Task.Run(async () =>
{
	// Small delay to let the Blazor host finish initialization
	await Task.Delay(500);

	var exitCode = await new WasmTestRunnerBuilder()
		.AddAssembly(typeof(SampleXunitTests).Assembly)
		.AddXunit()
		.UseResultChannel(new ConsoleResultChannel())
		.Build()
		.RunAsync();

	Console.WriteLine($"Tests completed with exit code: {exitCode}");
});

await host.RunAsync();
