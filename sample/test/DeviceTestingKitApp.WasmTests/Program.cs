using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;

namespace DeviceTestingKitApp.WasmTests;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Creating the WASM test runner application:");
        Console.WriteLine(" - Blazor WASM visual test runner");

        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Add HttpClient for dependency injection
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        // Configure the test runner
        builder.UseVisualTestRunner(conf => conf
            .AddConsoleResultChannel()
            .AddTestAssembly(typeof(Program).Assembly)
            .AddTestAssemblies(typeof(DeviceTestingKitApp.BlazorLibrary.XunitTests.BlazorUnitTests).Assembly)
            .AddXunit());

        await builder.Build().RunAsync();
    }
}