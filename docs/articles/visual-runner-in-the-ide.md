# Testing with the Visual Runner

The visual test runner provides an interactive UI for running tests directly on-device. It is the simplest way to run tests during development — just run the test app like any other app, either via the CLI or in the IDE.

## How It Works

The visual runner is integrated into a .NET MAUI app using the `UseVisualTestRunner()` extension method. When the app launches, it displays a test explorer UI that lets you:

- Browse all discovered tests grouped by assembly and class
- Run all tests or select individual tests to run
- View test results with pass/fail status and error details
- Filter tests by name or status

## Screenshots

| | | |
|:-:|:-:|:-:|
|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/386c00fa-05f3-476c-ae08-2594bf06c211)|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/6044737c-aaa7-4272-b2e0-07d8e1a31d9d)|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/c23bd064-e8d5-4a81-832e-9306219a32e9)|

## Setup

Add the visual runner to your MAUI test app's `MauiProgram.cs`:

```csharp
var builder = MauiApp.CreateBuilder();
builder
    .ConfigureUITesting()
    .UseVisualTestRunner(conf => conf
        .EnableAutoStart(true)
        .AddTestAssembly(typeof(MyTests).Assembly)
        .AddXunit()
        .AddXunit3()
        .AddNUnit()
        .AddConsoleResultChannel()
        .AddTcpResultChannel(new TcpResultChannelOptions
        {
            HostNames = ["localhost"],
            Port = 16384
        }));
```

## Blazor WebAssembly Visual Runner

The visual runner also works in the browser as a Blazor WebAssembly app. It uses the same ViewModels (`HomeViewModel`, `TestAssemblyViewModel`, etc.) as the MAUI runner, with Blazor Razor components (`HomePage.razor`, `TestAssemblyPage.razor`, `TestResultPage.razor`) providing the UI.

### Setup

Add the visual runner to your Blazor WebAssembly test app's `Program.cs`:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf => conf
    .AddXunit(useReflection: true)
    .AddXunit3()
    .AddTestAssembly(typeof(MyTests).Assembly)
    .AddConsoleResultChannel());

await builder.Build().RunAsync();
```

Key differences from the MAUI runner:
- Uses `WebAssemblyHostBuilder` instead of `MauiApp.CreateBuilder()`
- Requires `useReflection: true` for xUnit v2 since `XunitFrontController` needs filesystem access
- xUnit v3 works with plain `.AddXunit3()` — WASM compatibility is handled automatically
- The `UseVisualTestRunner` extension automatically calls `AddCliConfiguration()` which parses the page URL for `?device-runners-autorun=1` to support headless CLI execution
- No TCP result channel — uses console output with `EventStreamFormatter` for CLI integration

## See Also

- **[Technical Architecture Overview](technical-architecture-overview.md)** - Full details on the visual runner architecture
- **[Types of Tests](types-of-tests.md)** - Understanding different testing approaches
- **[Using DeviceRunners CLI](using-devicerunners-cli.md)** - Running tests from the command line
