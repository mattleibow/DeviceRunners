using DeviceRunners.Testing.Platform;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.UseTestingPlatformRunner(mtp =>
{
	mtp.AddXunit3();
	mtp.AddCliConfiguration();
});

await builder.Build().RunAsync();
