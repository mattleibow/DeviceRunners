# DeviceRunners Technical Architecture Overview

> [!NOTE]
> This documentation was partially generated using AI and may contain mistakes or be missing information. Please verify commands and procedures before use, and report any issues or improvements needed.

This page provides a comprehensive technical overview of all DeviceRunners components, features, and architectures. This serves as a reference for understanding the entire ecosystem before diving into specific documentation.

## Overview

DeviceRunners is a comprehensive testing framework for .NET MAUI applications that supports multiple testing approaches across different platforms. The project consists of two main testing strategies:

1. **Visual Test Runners** - Interactive UI-based test execution for development and manual testing
2. **CLI Test Runners** - Automated command-line test execution for CI/CD and scripted testing

## Supported Platforms

| Platform | Visual Runner | CLI Runner (XHarness) | CLI Runner (New Tool) |
|----------|---------------|----------------------|----------------------|
| **Android** | ✅ | ✅ | ✅ |
| **iOS** | ✅ | ✅ | ❌ |
| **macOS (Mac Catalyst)** | ✅ | ✅ | ✅ |
| **Windows (WinUI 3)** | ✅ | ❌ | ✅ |

## Supported Testing Frameworks

| Framework | Visual Runner | XHarness Runner | New CLI Tool |
|-----------|---------------|-----------------|--------------|
| **Xunit** | ✅ | ✅ | ❌ |
| **NUnit** | ✅ | ❌ | ❌ |

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
- **DeviceRunners.VisualRunners.Xunit** - Xunit test runner and discoverer
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
  - Windows: `DefaultAndroidEntryPoint` integration

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

**Network Commands:**
- `listen` - Start TCP port listener for test results

#### CLI Tool Features
- JSON and verbose output modes (`--output json`)
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
- **DeviceRunners.UITesting.Xunit** - Xunit UI testing support
  - `UIFact` and `UITheory` attributes
  - `UITestRunner`, `UITestCaseRunner` - Custom test execution
  - `UIFactDiscoverer`, `UITheoryDiscoverer` - Test discovery

#### UI Testing MAUI Integration (`DeviceRunners.UITesting.Maui`)
- MAUI-specific UI thread coordination
- Integration via `ConfigureUITesting()` extension method

## Test Execution Patterns

### Visual Runner Pattern
```csharp
var builder = MauiApp.CreateBuilder();
builder.UseVisualTestRunner(conf => conf
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
- **File Channel** - File-based result output
- **Custom Channels** - Extensible channel system

## Configuration Management

### Visual Test Runner Configuration
- Assembly discovery and loading
- Framework-specific discoverer registration  
- Result channel configuration
- Auto-start and auto-terminate options

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
- Multi-framework test projects (Xunit + NUnit)
- UI testing patterns
- Visual and XHarness runner configurations
- Platform-specific implementations
- MAUI library testing

## Future Architecture Considerations

The architecture is designed for extensibility:
- New testing frameworks can be added via ITestRunner/ITestDiscoverer
- New platforms can be supported via platform-specific implementations
- New result channels can be added for different output needs
- CLI tool can be extended with additional platform commands

This technical overview provides the foundation for understanding how all DeviceRunners components work together to provide comprehensive testing capabilities across platforms and testing scenarios.
