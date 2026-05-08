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
        .AddNUnit()
        .AddConsoleResultChannel()
        .AddTcpResultChannel(new TcpResultChannelOptions
        {
            HostNames = ["localhost"],
            Port = 16384
        }));
```

## See Also

- **[Technical Architecture Overview](technical-architecture-overview.md)** - Full details on the visual runner architecture
- **[Types of Tests](types-of-tests.md)** - Understanding different testing approaches
- **[Using DeviceRunners CLI](using-devicerunners-cli.md)** - Running tests from the command line
