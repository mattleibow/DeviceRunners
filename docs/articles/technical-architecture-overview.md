# DeviceRunners Technical Architecture Overview


This page provides a comprehensive technical overview of all DeviceRunners components, features, and architectures. This serves as a reference for understanding the entire ecosystem before diving into specific documentation.

## Overview

DeviceRunners is a comprehensive testing framework for .NET MAUI applications that supports multiple testing approaches across different platforms. The project consists of three main testing strategies:

1. **`dotnet test` Integration** - Standard .NET test workflow via the `DeviceRunners.Testing.Targets` NuGet package (recommended)
2. **Visual Test Runners** - Interactive UI-based test execution for development and manual testing
3. **CLI Test Runners** - Automated command-line test execution for advanced scenarios

## Supported Platforms

| Platform | `dotnet test` | Visual Runner | CLI Runner | XHarness (Legacy) |
|----------|:---:|:---:|:---:|:---:|
| **Android** | ✅ | ✅ | ✅ | ✅ |
| **iOS** | ✅ | ✅ | ✅ | ✅ |
| **macOS (Mac Catalyst)** | ✅ | ✅ | ✅ | ✅ |
| **Windows (WinUI 3)** | ✅ | ✅ | ✅ | ✅ |
| **Browser (WASM)** | ✅ | ✅ | ✅ | ❌ |

## Supported Testing Frameworks

| Framework | Visual Runner | `dotnet test` / CLI | XHarness Runner |
|-----------|:---:|:---:|:---:|
| **Xunit v2** | ✅ | ✅ (any) | ✅ |
| **Xunit v3** | ✅ | ✅ (any) | ❌ |
| **NUnit** | ✅ | ✅ (any) | ❌ |

> [!NOTE]
> Both `dotnet test` and the CLI tool are framework-agnostic — they launch the test app and collect results via TCP. They work with any testing framework that the app supports.

## Browser (WASM) Architecture

The WASM platform uses a fundamentally different architecture from the native platforms:

- **Blazor WebAssembly host**: The test app is a Blazor WebAssembly app built with `WebAssemblyHostBuilder` and `UseVisualTestRunner()`. It shares the same ViewModels (`HomeViewModel`, `TestAssemblyViewModel`, etc.) as the MAUI visual runner, with Blazor Razor components providing the UI.
- **Reflection-based discovery (xUnit v2)**: The standard `XunitFrontController` requires filesystem access to locate assemblies. In the browser, `XunitReflectionTestDiscoverer` scans assemblies already loaded in memory via reflection (`AddXunit(useReflection: true)`).
- **Automatic in-memory support (xUnit v3)**: xUnit v3 uses `Assembly.Location` for assembly identification, which is empty on Android, iOS, and WASM. `InMemoryXunit3TestAssembly` provides a logical path, and `InMemoryXunit3TestFramework` auto-detects this — no special flags needed (just `AddXunit3()`). See [xUnit v3 Support](xunit-v3-support.md#platform-compatibility-in-memory-assembly-handling) for details.
- **Cooperative yielding**: Blazor WebAssembly is single-threaded. The xunit runners (`XunitYieldingAssemblyRunner`, `XunitYieldingCollectionRunner`, `XunitYieldingClassRunner`) call `Task.Yield()` between test classes to give the browser event loop time for rendering.
- **Console output via EventStreamFormatter**: Since TCP sockets are not available from WebAssembly, test results are written as NDJSON lines to `console.log` using `EventStreamFormatter`. The DeviceRunners CLI captures this output through the Chrome DevTools Protocol (`Runtime.consoleAPICalled`).
- **CLI orchestration**: The `device-runners wasm test` command serves the published `wwwroot`, launches headless Chrome via CDP, navigates to `?device-runners-autorun=1`, captures console NDJSON events, and writes a TRX results file.

## Core Architecture Components

### 1. Core Libraries (`DeviceRunners.Core`)
- Base interfaces and shared functionality
- Device abstraction layer
- Result channel management
- Application termination handling

### 2. Visual Test Runners

#### Visual Runner Core (`DeviceRunners.VisualRunners`)
- **ITestRunner** interface - Core test execution
- **ITestDiscoverer** interface - Test discovery
- **CompositeTestRunner** - Runs multiple test framework runners
- **CompositeTestDiscoverer** - Discovers tests across frameworks
- **IVisualTestRunnerConfiguration** - Configuration management

#### Framework-Specific Visual Runners
- **DeviceRunners.VisualRunners.Xunit** - Xunit v2 test runner and discoverer
- **DeviceRunners.VisualRunners.Xunit3** - Xunit v3 test runner and discoverer (with automatic WASM support)
- **DeviceRunners.VisualRunners.NUnit** - NUnit test runner and discoverer

#### MAUI Integration (`DeviceRunners.VisualRunners.Maui`)
- MAUI app integration via `UseVisualTestRunner()`
- Visual UI components (pages, view models)
- Automatic runner detection and configuration
- Cross-platform UI implementation

### 3. XHarness Test Runners (Legacy CLI)

#### XHarness Core (`DeviceRunners.XHarness`)
- **ITestRunner** interface for XHarness execution
- **XHarnessDetector** - Auto-detection of XHarness environment
- Platform-specific entry points

#### XHarness Framework Integration
- **DeviceRunners.XHarness.Xunit** - Xunit support for XHarness
- Platform-specific implementations:
  - Android: `DefaultAndroidEntryPoint` integration
  - iOS/Mac Catalyst: `iOSApplicationEntryPoint` integration  
  - Windows: `DefaultAndroidEntryPoint` integration (reuses the Android base class)

#### XHarness MAUI Integration (`DeviceRunners.XHarness.Maui`)
- MAUI app integration via `UseXHarnessTestRunner()`
- Android instrumentation support
- Auto-configuration based on environment variables

### 4. New CLI Tool (`DeviceRunners.Cli`)

A modern cross-platform CLI tool that replaces platform-specific PowerShell scripts with unified commands.

#### Supported Commands

**Windows Commands:**
- `windows cert install` - Install certificates for MSIX packages
- `windows cert uninstall` - Remove certificates  
- `windows install` - Install MSIX applications
- `windows uninstall` - Uninstall applications
- `windows launch` - Launch applications
- `windows test` - Run tests (supports both .msix and .exe files)

**Android Commands:**
- `android install` - Install APK packages
- `android uninstall` - Uninstall applications
- `android launch` - Launch applications  
- `android test` - Run tests with logcat capture

**macOS Commands:**
- `macos install` - Install .app bundles
- `macos uninstall` - Uninstall applications
- `macos launch` - Launch applications
- `macos test` - Run tests

**iOS Commands:**
- `ios install` - Install .app bundles to simulator
- `ios uninstall` - Uninstall applications from simulator
- `ios launch` - Launch applications on simulator
- `ios test` - Run tests on simulator

**WASM Commands:**
- `wasm test` - Serve WASM app, run tests in headless Chrome, produce TRX results
- `wasm serve` - Serve WASM app for interactive browser testing

**Network Commands:**
- `listen` - Start TCP port listener for test results

#### CLI Tool Features
- JSON and verbose output modes (`--output json`)
- XML and text output modes (`--output xml`, `--output text`)
- TCP result listening for test execution
- Automatic certificate management (Windows)
- Device/emulator targeting (Android)
- Comprehensive error handling and cleanup
- Cross-platform using .NET and Spectre.Console

### 5. UI Testing Support

#### UI Testing Core (`DeviceRunners.UITesting`)
- **UIThreadCoordinator** - Thread coordination for UI tests
- Cross-platform UI thread abstraction

#### UI Testing Framework Integration
- **DeviceRunners.UITesting.Xunit** - Xunit v2 UI testing support
  - `UIFact` and `UITheory` attributes
  - `UITestRunner`, `UITestCaseRunner` - Custom test execution
  - `UIFactDiscoverer`, `UITheoryDiscoverer` - Test discovery
- **DeviceRunners.UITesting.Xunit3** - Xunit v3 UI testing support
  - `UIFact` and `UITheory` attributes (xUnit v3 compatible)
  - `DeviceTest` base class with `IDeviceTestApp` injection
  - Extension methods: `UseXunit3DeviceRunner()` and `AddDeviceTestApp()`

#### UI Testing MAUI Integration (`DeviceRunners.UITesting.Maui`)
- MAUI-specific UI thread coordination
- Integration via `ConfigureUITesting()` extension method

## Test Execution Patterns

### Visual Runner Pattern
```csharp
var builder = MauiApp.CreateBuilder();
builder.UseVisualTestRunner(conf => conf
    .EnableAutoStart(true)
    .AddTestAssembly(typeof(MyTests).Assembly)
    .AddXunit()
    .AddXunit3()
    .AddNUnit()
    .AddConsoleResultChannel()
    .AddFileResultChannel(new FileResultChannelOptions { Directory = "test-results" })
    .AddTcpResultChannel(new TcpResultChannelOptions
    {
        HostNames = ["localhost", "10.0.2.2"], // 10.0.2.2 is the Android emulator's alias for the host machine
        Port = 16384,
        Formatter = new EventStreamFormatter(),
        Required = false,
        Retries = 3,
        RetryTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(30)
    }));
```

### XHarness Runner Pattern  
```csharp
var builder = MauiApp.CreateBuilder();
builder.UseXHarnessTestRunner(conf => conf
    .AddTestAssembly(typeof(MyTests).Assembly)
    .AddXunit());
```

### UI Testing Pattern
```csharp
[UIFact]
public void TestUIComponent()
{
    // Test runs on UI thread automatically
    var entry = new Entry { Text = "test" };
    var handler = entry.ToHandler(MauiContext);
    Assert.Equal("test", handler.PlatformView.Text);
}
```

## Result Channel System

The framework supports multiple result output channels:

- **Console Channel** - Console output
- **TCP Channel** - Network-based result streaming  
- **File Channel** - File-based result output (via `AddFileResultChannel()`)
- **Custom Channels** - Extensible channel system

## Configuration Management

### Visual Test Runner Configuration
- Assembly discovery and loading (via `AddTestAssembly()` and `AddTestAssemblies()`)
- Framework-specific discoverer registration  
- Result channel configuration
- Auto-start and auto-terminate options (via `EnableAutoStart()`)

### XHarness Test Runner Configuration
- Environment variable detection
- Test assembly registration
- Skip category configuration
- Output directory management

## Platform-Specific Implementation Details

### Android
- Uses Android instrumentation for XHarness
- ADB integration for CLI tool
- Logcat capture and management
- Emulator and device targeting

### iOS/macOS
- Uses XHarness entry points
- App bundle management
- Simulator and device support
- Certificate handling (macOS)

### Windows  
- MSIX package support
- Certificate management and installation
- Dependency installation
- Both packaged (.msix) and unpackaged (.exe) app support
- PowerShell script automation converted to C#

## Testing Framework Integration

### Xunit Integration
- Custom test case runners for UI thread execution
- Theory and fact attribute support
- Parallel execution configuration
- Long-running test detection

### NUnit Integration  
- NUnit test assembly building
- Test listener implementation
- Result aggregation and reporting
- Filter support for test selection

## Development and Extension Points

### Creating Custom Test Runners
Implement `ITestRunner` interface:
```csharp
public class CustomTestRunner : ITestRunner
{
    public Task RunTestsAsync(IEnumerable<ITestAssemblyInfo> testAssemblies, CancellationToken cancellationToken = default);
    public Task RunTestsAsync(IEnumerable<ITestCaseInfo> testCases, CancellationToken cancellationToken = default);
}
```

### Creating Custom Test Discoverers
Implement `ITestDiscoverer` interface:
```csharp
public class CustomTestDiscoverer : ITestDiscoverer  
{
    public Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default);
}
```

### Adding Custom Result Channels
Extend the result channel system for custom output formats and destinations.

## Migration Notes

### From PowerShell Scripts to CLI Tool
The new CLI tool (`DeviceRunners.Cli`) replaces the previous PowerShell-based Windows testing scripts with:
- Cross-platform .NET implementation
- Unified command structure across platforms
- Better error handling and reporting
- JSON output support for automation
- TCP result streaming

### From Framework-Specific Runners
The visual runner system provides a unified approach that replaces framework-specific device runners with:
- Multi-framework support in single apps
- Consistent UI across platforms  
- Unified configuration and result handling
- Better development-time testing experience

## Sample Projects

The repository includes comprehensive sample projects demonstrating:
- Multi-framework test projects (Xunit v2 + Xunit v3 + NUnit)
- UI testing patterns
- Visual and XHarness runner configurations
- `dotnet test` integration via `DeviceRunners.Testing.Targets`
- Platform-specific implementations
- MAUI library testing
- Blazor WebAssembly browser testing (with all three frameworks)

## DeviceRunners.Testing.Targets Package

The `DeviceRunners.Testing.Targets` NuGet package enables `dotnet test` for device platforms by replacing the standard VSTest MSBuild target with a custom implementation that:

1. **Builds** the app for the target platform
2. **Deploys** it using the bundled DeviceRunners CLI tool
3. **Launches** the app with configuration for auto-run and TCP connection
4. **Collects** test results via TCP and writes a TRX file
5. **Reports** results in the standard `dotnet test` output format

### Package Structure

The package ships two MSBuild files and self-contained CLI binaries:

```
build/
  DeviceRunners.Testing.Targets.props    # Imported early: disables MTP, sets defaults
  DeviceRunners.Testing.Targets.targets  # Imported late: custom VSTest target chain
tools/
  osx-arm64/DeviceRunners.Cli            # Self-contained single-file binary (~20 MB)
  osx-x64/DeviceRunners.Cli
  win-x64/DeviceRunners.Cli.exe
  win-arm64/DeviceRunners.Cli.exe
  linux-x64/DeviceRunners.Cli
  linux-arm64/DeviceRunners.Cli
```

### MSBuild Target Chain

```
VSTest  (entry point, replaces SDK default)

  -> Build                        (compile the app)
  -> _DeviceRunnersRunTests       (orchestrator)
       -> _DeviceRunnersPrepareArgs    (common + platform-specific CLI args)
       -> _DeviceRunnersExecTests      (single Exec, captures exit code)
       -> _DeviceRunnersReportResults  (parse TRX, emit summary)
```

Platform detection uses `$(_DeviceRunnersPlatform)` computed once from `GetTargetPlatformIdentifier`. Each platform has its own args target that assembles the CLI command. The `_DeviceRunnersExecTests` target runs a single `Exec` with `IgnoreExitCode="true"` and captures the exit code for clean error reporting.

### Exit Code Protocol

| Code | Meaning | MSBuild Output |
|------|---------|----------------|
| 0 | All tests passed | No error |
| 1 | Test failures | `error TESTERROR: Test summary: ...` |
| 2 | App crashed | `error TESTERROR: Test summary: ... (incomplete: app crashed)` |

For more details, see [Using dotnet test](using-dotnet-test.md).

## Future Architecture Considerations

The architecture is designed for extensibility:
- New testing frameworks can be added via ITestRunner/ITestDiscoverer
- New platforms can be supported via platform-specific implementations
- New result channels can be added for different output needs
- CLI tool can be extended with additional platform commands

This technical overview provides the foundation for understanding how all DeviceRunners components work together to provide comprehensive testing capabilities across platforms and testing scenarios.
