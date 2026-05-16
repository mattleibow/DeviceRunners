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

DeviceRunners.VisualRunners.Xunit3 uses xUnit v3's in-process extensibility APIs directly:

1. **Discovery**: Uses `ExtensibilityPointFactory.GetTestFramework(assembly)` to obtain the xUnit v3 framework, then calls `ITestFrameworkDiscoverer.Find()` with a callback to collect discovered test cases
2. **Execution**: Uses `ITestFrameworkExecutor.RunTestCases()` with the previously discovered `ITestCase` objects filtered to the selected tests
3. **Results**: Implements `IMessageSink` to receive `ITestPassed`, `ITestFailed`, `ITestSkipped`, and `ITestNotRun` messages and map them to DeviceRunners' result model
4. **Diagnostics**: Framework diagnostic messages are forwarded to `IDiagnosticsManager` when available
5. **Error handling**: Framework-level errors (`IErrorMessage`, cleanup failures) are surfaced through diagnostics

All execution happens in-process on the device — no separate test process is launched.

## Current Limitations

- **Test Explorer**: Device test projects using xUnit v3 cannot be run via `dotnet test` or Visual Studio Test Explorer (this is a fundamental xUnit v3 architecture limitation for device testing)

## UI Testing

DeviceRunners also provides `[UIFact]` and `[UITheory]` attributes for xUnit v3 via the `DeviceRunners.UITesting.Xunit3` package. These work the same as their xUnit v2 counterparts — test methods decorated with these attributes will be dispatched to the UI thread for execution.

```csharp
using Xunit;

public class MyUITests
{
    [UIFact]
    public void TestOnUIThread()
    {
        // This runs on the UI thread
    }

    [UITheory]
    [InlineData(1)]
    [InlineData(2)]
    public void TheoryOnUIThread(int value)
    {
        // This also runs on the UI thread
    }
}
```

## WASM / Blazor Browser Support

xUnit v3 works on WebAssembly (Blazor) with automatic platform detection — no special flags needed. Just use `.AddXunit3()`:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf => conf
    .AddXunit(useReflection: true)   // xUnit v2 needs reflection mode
    .AddXunit3()                      // xUnit v3 works automatically
    .AddTestAssembly(typeof(MyXunit2Tests).Assembly)
    .AddTestAssemblies(typeof(MyXunit3Tests).Assembly)
    .AddConsoleResultChannel());

await builder.Build().RunAsync();
```

### How It Works on WASM

On WebAssembly, `Assembly.Location` returns an empty string because there is no filesystem. xUnit v3's `XunitTestAssembly` uses `Assembly.Location` as its `AssemblyPath`, which causes `TestAssemblyRunner.OnTestAssemblyStarting` to crash (it calls `Path.GetFileNameWithoutExtension(AssemblyPath)` on the empty string).

DeviceRunners detects this automatically and uses WASM-safe replacements:

- **`WasmXunit3TestAssembly`** — Subclass of `XunitTestAssembly` that re-implements the `IXunitTestAssembly` interface, providing a logical assembly path (`AssemblyName + ".dll"`) instead of the empty `Assembly.Location`. The interface must be re-declared on the subclass to force C# interface dispatch remapping, since `XunitTestAssembly.AssemblyPath` is not virtual.
- **`WasmXunit3TestFramework`** — Subclass of `XunitTestFramework` that overrides `CreateDiscoverer` and `CreateExecutor` to use `WasmXunit3TestAssembly` when on WASM.

The `Xunit3TestDiscoverer` and `Xunit3TestRunner` both use a `CreateTestFramework()` helper that checks `Assembly.Location` at runtime:
- **Empty** → creates `WasmXunit3TestFramework` (WASM path)
- **Non-empty** → uses `ExtensibilityPointFactory.GetTestFramework()` (standard path)

### Desktop vs WASM Differences

| Aspect | Desktop / Device (MAUI) | WASM (Blazor) |
|---|---|---|
| **Setup** | `.AddXunit3()` | `.AddXunit3()` (same) |
| **Assembly location** | `Assembly.Location` returns file path | `Assembly.Location` is empty string |
| **Test framework** | Standard `XunitTestFramework` via `ExtensibilityPointFactory` | `WasmXunit3TestFramework` (auto-detected) |
| **Test assembly** | `XunitTestAssembly` | `WasmXunit3TestAssembly` (provides logical path) |
| **Threading** | Multi-threaded, tests run on thread pool | Single-threaded, cooperative execution |
| **Result output** | TCP socket + console | Console NDJSON via `EventStreamFormatter` |
| **Configuration** | `xunit.runner.json` from file system | `xunit.runner.json` from app package resources |
| **`[TestFramework]` attribute** | Supported (via `ExtensibilityPointFactory`) | Not supported (bypassed on WASM) |

> **Note:** Unlike xUnit v2 which requires `useReflection: true` on WASM (because `XunitFrontController` needs filesystem access), xUnit v3 works with plain `.AddXunit3()` on all platforms. The WASM workaround is internal and transparent.

### Comparison with xUnit v2 WASM Approach

xUnit v2 on WASM requires a completely different discoverer (`XunitReflectionTestDiscoverer`) and runner (`XunitReflectionTestRunner`) because `XunitFrontController` depends on file paths to load assemblies. The reflection-based approach bypasses `XunitFrontController` entirely and scans assemblies already loaded in memory.

xUnit v3 takes a different approach: the same discoverer and runner work on all platforms. Only the `IXunitTestAssembly` instance is swapped to provide a logical path, and the `ITestFramework` creation is redirected to avoid `ExtensibilityPointFactory` (which also uses file paths internally). This is a smaller and more targeted workaround.

## Differences from xUnit v2

| Feature | xUnit v2 (`AddXunit()`) | xUnit v3 (`AddXunit3()`) |
|---|---|---|
| Package for tests | `xunit` | `xunit.v3.extensibility.core` + `xunit.v3.assert` |
| Test discovery API | `XunitFrontController.Find()` | `ExtensibilityPointFactory` + `ITestFrameworkDiscoverer.Find()` |
| Test execution API | `XunitFrontController.RunTests()` | `ITestFrameworkExecutor.RunTestCases()` |
| Message handling | Event-based `TestMessageSink` | `IMessageSink.OnMessage()` |
| Selective execution | `ITestCase` object references | Re-discover + filter by unique ID |
| Configuration | Loads `xunit.runner.json` | Loads `xunit.runner.json` |
| UI testing attributes | `DeviceRunners.UITesting.Xunit` | `DeviceRunners.UITesting.Xunit3` |
| WASM support | Requires `useReflection: true` | Automatic (transparent WASM detection) |
| `IAsyncLifetime` | Returns `Task` | Returns `ValueTask` |
