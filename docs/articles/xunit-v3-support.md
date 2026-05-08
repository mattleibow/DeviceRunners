# xUnit v3 Support

DeviceRunners supports xUnit v3 alongside xUnit v2 and NUnit for device testing. This guide covers how to set up and use xUnit v3 with DeviceRunners.

## Package Architecture

xUnit v3 changed its architecture so that test projects are executables. For device testing, where the MAUI app is the host, test assemblies need to remain as **class libraries**. DeviceRunners handles this by using the lower-level xUnit v3 packages:

| Your test library references | Purpose |
|---|---|
| `xunit.v3.extensibility.core` | Test attributes (`[Fact]`, `[Theory]`, etc.) |
| `xunit.v3.assert` | `Assert.*` methods |

> **Important:** Do NOT reference `xunit.v3` or `xunit.v3.core` in your test class libraries. Those packages force the project to become an executable and inject a `Main` method, which conflicts with the MAUI app host.

## Quick Start

### 1. Create a Test Class Library

Create a standard .NET class library for your tests:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit.v3.extensibility.core" />
    <PackageReference Include="xunit.v3.assert" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\YourApp\YourApp.csproj" />
  </ItemGroup>
</Project>
```

### 2. Write Tests

Write tests exactly as you would with xUnit v3:

```csharp
using Xunit;

public class MyTests
{
    [Fact]
    public void BasicTest()
    {
        Assert.True(true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ParameterizedTest(int value)
    {
        Assert.NotEqual(0, value);
    }

    [Fact(Skip = "Not ready yet")]
    public void SkippedTest() { }
}
```

### 3. Configure the MAUI Test Runner

In your MAUI test app's `MauiProgram.cs`, add the xUnit v3 runner:

```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseVisualTestRunner(conf => conf
        .AddConsoleResultChannel()
        .AddTestAssembly(typeof(MauiProgram).Assembly)
        .AddTestAssemblies(typeof(MyTests).Assembly)
        .AddXunit3());  // Enable xUnit v3 support
```

You can use `.AddXunit3()` alongside `.AddXunit()` and `.AddNUnit()` to run tests from multiple frameworks simultaneously.

## Using Multiple Test Frameworks

DeviceRunners supports running xUnit v2, xUnit v3, and NUnit tests together:

```csharp
builder.UseVisualTestRunner(conf => conf
    .AddConsoleResultChannel()
    .AddTestAssembly(typeof(MauiProgram).Assembly)
    .AddTestAssemblies(typeof(XunitV2Tests).Assembly)
    .AddTestAssemblies(typeof(Xunit3Tests).Assembly)
    .AddTestAssemblies(typeof(NUnitTests).Assembly)
    .AddXunit()    // xUnit v2
    .AddXunit3()   // xUnit v3
    .AddNUnit());  // NUnit
```

Each framework's discoverer will only find and run tests from its own framework. Tests from different frameworks can coexist in the visual runner UI.

## How It Works

DeviceRunners.VisualRunners.Xunit3 uses xUnit v3's in-process execution APIs:

1. **Discovery**: Uses `ConsoleRunnerInProcess.Find()` to discover tests in the loaded assembly
2. **Execution**: Uses `ConsoleRunnerInProcess.Run()` with `TestCaseIDsToRun` for selective test execution
3. **Results**: Implements `IMessageSink` to receive `ITestPassed`, `ITestFailed`, `ITestSkipped` messages and map them to DeviceRunners' result model

All execution happens in-process on the device — no separate test process is launched.

## Configuration

xUnit v3 configuration is loaded from `xunit.runner.json` files, following the same convention as xUnit v2:

1. `{AssemblyName}.xunit.runner.json` (assembly-specific)
2. `xunit.runner.json` (global)

See the [xUnit v3 configuration documentation](https://xunit.net/docs/config-xunit-runner-json) for available settings.

## Differences from xUnit v2

| Feature | xUnit v2 (`AddXunit()`) | xUnit v3 (`AddXunit3()`) |
|---|---|---|
| Package for tests | `xunit` | `xunit.v3.extensibility.core` + `xunit.v3.assert` |
| Test discovery API | `XunitFrontController.Find()` | `ConsoleRunnerInProcess.Find()` |
| Test execution API | `XunitFrontController.RunTests()` | `ConsoleRunnerInProcess.Run()` |
| Message handling | Event-based `TestMessageSink` | `IMessageSink.OnMessage()` |
| Selective execution | `ITestCase` objects | Test case unique IDs |
| `IAsyncLifetime` | Returns `Task` | Returns `ValueTask` |

## Limitations

- **XHarness**: xUnit v3 XHarness integration is not yet available (no official XHarness v3 packages)
- **Test Explorer**: Device test projects using xUnit v3 cannot be run via `dotnet test` or Visual Studio Test Explorer (this is a fundamental xUnit v3 architecture limitation for device testing)
- **UI Testing**: xUnit v3 UI testing integration (`DeviceRunners.UITesting.Xunit3`) is planned for a future release
