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

1. **Discovery**: Uses `ExtensibilityPointFactory.GetTestFramework(assembly)` (or `InMemoryXunit3TestFramework` on platforms where `Assembly.Location` is empty) to obtain the xUnit v3 framework, then calls `ITestFrameworkDiscoverer.Find()` with a callback to collect discovered test cases
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

## Platform Compatibility (In-Memory Assembly Handling)

xUnit v3 works on **all platforms** with automatic detection — no special flags needed. Just use `.AddXunit3()`:

```csharp
// MAUI (Android, iOS, macOS, Windows)
builder.UseVisualTestRunner(conf => conf
    .AddXunit3()
    .AddTestAssembly(typeof(MyTests).Assembly));

// Blazor WebAssembly
builder.UseVisualTestRunner(conf => conf
    .AddXunit(useReflection: true)   // xUnit v2 needs reflection mode on WASM
    .AddXunit3()                      // xUnit v3 works automatically everywhere
    .AddTestAssembly(typeof(MyXunit2Tests).Assembly)
    .AddTestAssemblies(typeof(MyXunit3Tests).Assembly)
    .AddConsoleResultChannel());
```

### The `Assembly.Location` Problem

On several platforms, `Assembly.Location` returns an **empty string** because assemblies are loaded from streams or bundles rather than from disk files:

| Platform | `Assembly.Location` | Why |
|---|---|---|
| **Windows** | ✅ File path | DLLs on disk |
| **macOS (Catalyst)** | ✅ File path | DLLs in app bundle |
| **Android** | ❌ Empty string | DLLs inside APK (zip stream) |
| **iOS** | ❌ Empty string | DLLs in app bundle (AOT/stream) |
| **WASM** | ❌ Empty string | DLLs loaded as byte arrays |

xUnit v3's `XunitTestAssembly` uses `Assembly.Location` as its `AssemblyPath`, which causes `TestAssemblyRunner.OnTestAssemblyStarting` to crash when it's empty (it calls `Path.GetFileNameWithoutExtension(AssemblyPath)` on the empty string).

### How DeviceRunners Handles This

DeviceRunners detects `Assembly.Location` at runtime and automatically uses in-memory replacements:

- **`InMemoryXunit3TestAssembly`** — Subclass of `XunitTestAssembly` that re-implements the `IXunitTestAssembly` interface, providing a logical assembly path (`AssemblyName + ".dll"`) instead of the empty `Assembly.Location`. The interface must be re-declared on the subclass to force C# interface dispatch remapping, since `XunitTestAssembly.AssemblyPath` is not virtual.
- **`InMemoryXunit3TestFramework`** — Subclass of `XunitTestFramework` that overrides `CreateDiscoverer` and `CreateExecutor` to use `InMemoryXunit3TestAssembly` when `Assembly.Location` is empty.

The `Xunit3TestDiscoverer` and `Xunit3TestRunner` both use a `CreateTestFramework()` helper that checks `Assembly.Location` at runtime:
- **Empty** → creates `InMemoryXunit3TestFramework` (in-memory path — Android, iOS, WASM)
- **Non-empty** → uses `ExtensibilityPointFactory.GetTestFramework()` (standard path — Windows, macOS)

### Desktop vs Device/WASM Differences

| Aspect | Desktop (Windows, macOS) | Device / WASM (Android, iOS, WASM) |
|---|---|---|
| **Setup** | `.AddXunit3()` | `.AddXunit3()` (same) |
| **Assembly location** | `Assembly.Location` returns file path | `Assembly.Location` is empty string |
| **Test framework** | `XunitTestFramework` via `ExtensibilityPointFactory` | `InMemoryXunit3TestFramework` (auto-detected) |
| **Test assembly** | `XunitTestAssembly` | `InMemoryXunit3TestAssembly` (logical path) |
| **`[TestFramework]` attribute** | Supported (via `ExtensibilityPointFactory`) | Not supported (bypassed when in-memory) |
| **Threading** | Multi-threaded | Multi-threaded (MAUI) / Single-threaded (WASM) |
| **Result output** | TCP socket + console | TCP (MAUI) / Console NDJSON (WASM) |

> **Note:** Unlike xUnit v2 which requires `useReflection: true` on platforms without filesystem access (because `XunitFrontController` needs file paths), xUnit v3 works with plain `.AddXunit3()` everywhere. The in-memory workaround is internal and transparent.

### Comparison with xUnit v2 Approach

xUnit v2 handles the `Assembly.Location` problem differently per platform:

- **Android**: `FileSystemUtils.GetAssemblyFileName()` creates a **dummy file** on disk so `XunitFrontController` has a valid path to open. This is wasteful but functional.
- **WASM**: A completely different discoverer (`XunitReflectionTestDiscoverer`) and runner (`XunitReflectionTestRunner`) bypass `XunitFrontController` entirely, using xUnit's internal reflection APIs to scan assemblies in memory.

xUnit v3 takes a cleaner approach: the **same discoverer and runner work on all platforms**. Only the `IXunitTestAssembly` instance is swapped to provide a logical path, and the `ITestFramework` creation is redirected to avoid `ExtensibilityPointFactory` (which also uses file paths internally). No dummy files, no alternate code paths.

## Differences from xUnit v2

| Feature | xUnit v2 (`AddXunit()`) | xUnit v3 (`AddXunit3()`) |
|---|---|---|
| Package for tests | `xunit` | `xunit.v3.extensibility.core` + `xunit.v3.assert` |
| Test discovery API | `XunitFrontController.Find()` | `ExtensibilityPointFactory` + `ITestFrameworkDiscoverer.Find()` |
| Test execution API | `XunitFrontController.RunTests()` | `ITestFrameworkExecutor.RunTestCases()` |
| Message handling | Event-based `TestMessageSink` | `IMessageSink.OnMessage()` |
| Selective execution | `ITestCase` object references | Cached `ITestCase` objects from discovery |
| Configuration | Loads `xunit.runner.json` | Loads `xunit.runner.json` |
| UI testing attributes | `DeviceRunners.UITesting.Xunit` | `DeviceRunners.UITesting.Xunit3` |
| WASM support | Requires `useReflection: true` | Automatic (transparent in-memory detection) |
| `IAsyncLifetime` | Returns `Task` | Returns `ValueTask` |
