using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Blazor.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf =>
{
	conf.AddXunit(useReflection: true);
	conf.AddTestAssembly(typeof(SampleXunitTests).Assembly);
	conf.EnableAutoStart(autoTerminate: true);
	conf.AddResultChannel(_ => new ConsoleResultChannel(new EventStreamFormatter()));
});

await builder.Build().RunAsync();
