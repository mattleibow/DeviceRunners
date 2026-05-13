using System.Runtime.InteropServices.JavaScript;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Blazor.Components;

var currentUrl = BrowserInterop.GetLocationHref();

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf => conf
	.AddCliConfiguration(currentUrl)
	.AddXunit(useReflection: true)
	.AddTestAssembly(typeof(SampleXunitTests).Assembly)
	.AddConsoleResultChannel());

await builder.Build().RunAsync();

static partial class BrowserInterop
{
	[JSImport("globalThis.getLocationHref")]
	internal static partial string GetLocationHref();
}
