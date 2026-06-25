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

Create a .NET class library for your tests. Multi-target it with your device platforms so the same library can be loaded by the MAUI visual runner:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0;net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
    <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net10.0-windows10.0.19041.0</TargetFrameworks>
    <UseMaui>true</UseMaui>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit.v3.extensibility.core" />
    <PackageReference Include="xunit.v3.assert" />
  </ItemGroup>
  <!-- Optional: enable 'dotnet test' on the host TFM (net10.0) -->
  <PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == ''">
    <OutputType>Exe</OutputType>
    <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
  </PropertyGroup>
  <ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == ''">
    <PackageReference Include="xunit.v3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\YourApp\YourApp.csproj" />
  </ItemGroup>
</Project>
```

> **Tip:** The conditional `xunit.v3` reference and `OutputType=Exe` on the host TFM lets you run `dotnet test -f net10.0` for quick local iteration. Device TFMs use the MAUI visual runner instead.

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

- **`[TestFramework]` attribute on in-memory platforms**: On platforms where `Assembly.Location` is empty (Android, iOS, WASM), DeviceRunners uses `InMemoryXunit3TestFramework` instead of calling `ExtensibilityPointFactory.GetTestFramework()`. This means any `[TestFramework]` assembly-level attribute that customizes the xUnit test framework will be **ignored** on those platforms. Custom test frameworks registered via `[TestFramework]` only work on desktop (Windows, macOS). This is tracked upstream at [xunit/xunit#3096](https://github.com/xunit/xunit/issues/3096) — if `XunitTestAssembly.AssemblyPath` is made virtual or `Assembly.Location` handling improves, this limitation can be removed.

## UI Testing

DeviceRunners also provides `[UIFact]` and `[UITheory]` attributes for xUnit v3 via the `DeviceRunners.UITesting.Xunit3` package. These work the same as their xUnit v2 counterparts — test methods decorated with these attributes will have the entire test lifecycle (class construction, `IAsyncLifetime`, test method invocation, and disposal) dispatched to the UI thread.

> **Same namespace as v2:** The v3 `[UIFact]` and `[UITheory]` attributes are in the `Xunit` namespace, matching both the v2 convention and the official xUnit v3 framework. Migrating from v2 to v3 requires only swapping the NuGet package — no namespace changes needed.

```csharp
using Xunit;

public class MyUITests
{
    [UIFact]
    public void TestOnUIThread()
    {
        // The entire test lifecycle runs on the UI thread:
        // construction, IAsyncLifetime, test method, and disposal
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

## Test Filtering (Microsoft Testing Platform parity)

When you run xUnit v3 tests through `dotnet test`, the [Microsoft Testing Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
exposes a set of typed "simple" filter switches. The DeviceRunners CLI accepts the same
switches on its `test` command and runs the matching subset on-device:

| Microsoft Testing Platform switch | DeviceRunners CLI equivalent |
|---|---|
| `--filter-class` / `--filter-not-class` | `--filter-class` / `--filter-not-class` |
| `--filter-method` / `--filter-not-method` | `--filter-method` / `--filter-not-method` |
| `--filter-namespace` / `--filter-not-namespace` | `--filter-namespace` / `--filter-not-namespace` |
| `--filter-trait` / `--filter-not-trait` | `--filter-trait` / `--filter-not-trait` |

Each switch is repeatable and supports `*` wildcards. Same-kind values OR together,
different kinds AND together, and the `not-` variants exclude. See
[Using DeviceRunners CLI](using-devicerunners-cli.md#filtering-tests) for examples. The
on-device evaluator applies the filter the same way for xUnit v2, xUnit v3 and NUnit, so the
simple filters behave identically regardless of which framework discovered the test.

> [!NOTE]
> The DeviceRunners `--filter` expression (and its `--filter-*` simple-filter translation)
> is evaluated by DeviceRunners' own on-device matcher, not by xUnit v3's filter engine. The
> switch **names** and combine semantics match Microsoft Testing Platform, but the matching
> happens against the framework-agnostic test metadata DeviceRunners collects.

### `--filter-query` (advanced graph query)

xUnit v3 also exposes an advanced `--filter-query` option that uses a path-like **graph
query language** instead of the simple switches. DeviceRunners does **not** implement
`--filter-query` — use the simple `--filter-*` switches (or the `--filter` expression)
instead. It is documented here only so the syntax is familiar if you see it elsewhere.

A query matches the tree `/<assembly>/<namespace>/<class>/<method>`, with an optional
`[trait=value]` suffix:

| Query | Matches |
|---|---|
| `/MyTests/MyApp.Calc/CalculatorTests/Adds` | The single `Adds` method |
| `/*/*/CalculatorTests/*` | Every test in `CalculatorTests` |
| `/*/MyApp.Calc/*/*` | Everything in the `MyApp.Calc` namespace |
| `/[Category=Smoke]` | Every test with trait `Category=Smoke` |
| `/[Category!=Slow]` | Every test that does **not** have `Category=Slow` |

`*` wildcards are allowed at the start/end of any segment, and `|`/`&` can combine patterns
within a single (parenthesized) segment. `--filter-query` cannot be mixed with the simple
filters. For the closest DeviceRunners equivalents, map class/namespace/method/trait segments
onto the matching `--filter-*` switch.

## Known Limitations

The DeviceRunners visual runner executes xUnit v3 tests in-process within a MAUI app. This is different from a standard xUnit v3 test project which runs as a standalone executable via `dotnet test`. Note that `dotnet test` still works for the **host TFM** (`net10.0`) of your test libraries — only the **device TFMs** use the in-process visual runner.

### Platform Workarounds (Visual Runner Only)

| Limitation | Details | Tracking |
|---|---|---|
| **`Assembly.Location` is empty** | On Android, iOS, and WASM, `Assembly.Location` returns an empty string. DeviceRunners works around this with `InMemoryXunit3TestAssembly` which provides a logical path. | [xunit/xunit#3577](https://github.com/xunit/xunit/issues/3577) |
| **Config file discovery** | Standard xUnit v3 loads `xunit.runner.json` from the filesystem next to the assembly DLL. DeviceRunners loads it from app package resources via `OpenAppPackageFile` instead, since assemblies may not be on disk. |
| **`[TestFramework]` attribute ignored on in-memory platforms** | When `Assembly.Location` is empty, DeviceRunners creates `InMemoryXunit3TestFramework` directly instead of using `ExtensibilityPointFactory`, which means any `[TestFramework]` assembly attribute is not honored. |

### Visual Runner Feature Gaps

These limitations apply only to the in-app visual runner, not to `dotnet test` on the host TFM:

| Feature | Standard `dotnet test` | DeviceRunners Visual Runner |
|---|---|---|
| **`[Fact(Explicit = true)]`** | ✅ Runs with `--filter` or explicit opt-in | ⚠️ Executor supports it, but the visual runner UI has no way to opt-in to running explicit tests |
| **`dotnet test --filter` expressions** | ✅ Full filter syntax | ✅ Headless runs honor `--filter` (a documented subset); the interactive UI also has its own filtering |
| **Source information** | ✅ IDE navigation to test source | ❌ Not available on in-memory platforms |

### Behavioral Defaults

DeviceRunners uses the standard xUnit v3 defaults for all configuration options. Users can customize behavior via `xunit.runner.json` (placed in app package resources). Notable defaults:

- **`PreEnumerateTheories`**: `false` (a `[Theory]` with 3 `[InlineData]` appears as 1 test case, not 3). Set to `true` in `xunit.runner.json` to see individual theory data rows in the visual runner.
- **`SynchronousMessageReporting`**: `false` (messages delivered asynchronously). The visual runner's message sink is thread-safe.
- **Parallelization**: Follows xUnit v3 defaults (parallel by collection). Configurable via `xunit.runner.json`.

### Verified xUnit v3 Features

The following xUnit v3 features have been tested through DeviceRunners' unit and device test suites:

- `[Fact]`, `[Theory]`, `[InlineData]`
- `IAsyncLifetime` (construction and disposal on UI thread for `[UIFact]`/`[UITheory]`)
- `IDisposable` test class cleanup
- `[Skip]`, `[Fact(Skip = "...")]`
- `ITestOutputHelper` (output captured and reported)
- `ISelfExecutingXunitTestCase` (used by `[UIFact]`/`[UITheory]`)
- Theory row aggregation (`PreEnumerateTheories=false`: a failing row marks the test case as failed)

The following are supported by xUnit v3 and expected to work through DeviceRunners, but do not have dedicated tests in this repository:

- `[MemberData]`, `[ClassData]`
- `[Collection]` for test serialization
- Test assembly parallelization control
- `xunit.runner.json` configuration loading
