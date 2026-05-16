using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Blazor.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf => conf
	.AddXunit(useReflection: true)
	.AddXunit3()
	.AddNUnit()
	.AddTestAssembly(typeof(DeviceTestingKitApp.BrowserTests.UnitTests).Assembly)
	.AddTestAssemblies(typeof(DeviceTestingKitApp.BlazorLibrary.XunitTests.UnitTests).Assembly)
	.AddTestAssemblies(typeof(DeviceTestingKitApp.BlazorLibrary.Xunit3Tests.UnitTests).Assembly)
	.AddTestAssemblies(typeof(DeviceTestingKitApp.BlazorLibrary.NUnitTests.UnitTests).Assembly)
	.AddConsoleResultChannel());

await builder.Build().RunAsync();
