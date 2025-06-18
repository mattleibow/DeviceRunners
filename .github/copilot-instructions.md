# DeviceRunners - GitHub Copilot Instructions

This repository contains **DeviceRunners**, a comprehensive cross-platform device testing framework for .NET applications. This document provides context and guidelines to help GitHub Copilot assist with development.

## Project Overview

DeviceRunners enables running unit tests, integration tests, and UI tests on real devices across multiple platforms (Android, iOS, macOS, Windows) using various testing frameworks (Xunit, NUnit). The project originated from migrating and modernizing existing testing solutions to .NET MAUI.

### Key Value Propositions
- **Device-native testing**: Run tests directly on target devices rather than just on development machines
- **Multi-platform support**: Single codebase supporting Android, iOS, macOS (Catalyst), and Windows
- **Multiple test runners**: Visual runners for interactive testing, CLI runners for CI/CD
- **Framework flexibility**: Support for Xunit, NUnit, and extensible for other frameworks

## Architecture Overview

### Core Components

1. **DeviceRunners.Core**: Shared foundation library containing platform-agnostic interfaces and base classes
2. **DeviceRunners.VisualRunners**: Interactive GUI test runners with real-time test execution and results
3. **DeviceRunners.XHarness**: CLI-based test runners integrating with Microsoft's XHarness tool for CI/CD
4. **DeviceRunners.UITesting**: UI testing infrastructure with Xunit integration
5. **DeviceRunners.Cli**: Cross-platform command-line tool for device operations (certificates, packages, testing)

### Testing Framework Integration

- **DeviceRunners.VisualRunners.Xunit**: Xunit integration for visual runners
- **DeviceRunners.VisualRunners.NUnit**: NUnit integration for visual runners
- **DeviceRunners.XHarness.Xunit**: Xunit integration for XHarness CLI runners

### Platform Support

```xml
<TargetFrameworks>net9.0;net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0</TargetFrameworks>
```

Each library uses conditional compilation and platform-specific folders:
- `Platforms/Android/` - Android-specific implementations
- `Platforms/iOS/` - iOS-specific implementations  
- `Platforms/MacCatalyst/` - macOS Catalyst implementations
- `Platforms/Windows/` - Windows/WinUI implementations
- `Platforms/Apple/` - Shared iOS/macOS implementations
- `Platforms/All/` - Cross-platform implementations

## Project Structure Conventions

### Source Organization
```
src/
├── DeviceRunners.Core/                 # Core abstractions and interfaces
├── DeviceRunners.VisualRunners/        # Base visual runner implementation
├── DeviceRunners.VisualRunners.Maui/   # MAUI-specific visual runner components
├── DeviceRunners.VisualRunners.Xunit/  # Xunit visual runner
├── DeviceRunners.VisualRunners.NUnit/  # NUnit visual runner
├── DeviceRunners.XHarness/             # Base XHarness runner
├── DeviceRunners.XHarness.Maui/        # MAUI XHarness integration
├── DeviceRunners.XHarness.Xunit/       # Xunit XHarness runner
├── DeviceRunners.UITesting/            # UI testing base
├── DeviceRunners.UITesting.Maui/       # MAUI UI testing
├── DeviceRunners.UITesting.Xunit/      # Xunit UI testing
└── DeviceRunners.Cli/                  # Command-line tool
```

### Test Organization
```
test/
├── DeviceRunners.Cli.Tests/            # CLI tool unit tests
├── DeviceRunners.VisualRunners.Tests/  # Visual runner unit tests
└── TestProject.Tests/                  # General test utilities
```

### Sample Applications
```
sample/
├── src/
│   ├── DeviceTestingKitApp/            # Main MAUI sample app
│   ├── DeviceTestingKitApp.Library/    # Framework-agnostic library
│   └── DeviceTestingKitApp.MauiLibrary/ # MAUI-specific library
└── test/
    ├── DeviceTestingKitApp.DeviceTests/           # Device-specific tests
    ├── DeviceTestingKitApp.Library.NUnitTests/    # NUnit tests
    └── DeviceTestingKitApp.MauiLibrary.XunitTests/ # Xunit tests
```

## Coding Conventions

### General .NET Standards
- **Nullable reference types**: Enabled project-wide (`<Nullable>enable</Nullable>`)
- **Implicit usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- **Target framework**: .NET 9.0 as baseline with platform-specific targets
- **Central Package Management**: All package versions managed in `Directory.Packages.props`

### Platform-Specific Code Patterns

Use conditional compilation for platform-specific implementations:

```csharp
#if ANDROID
    // Android-specific code
    return Android.App.Application.Context.CacheDir.AbsolutePath;
#elif IOS || MACCATALYST
    // iOS/macOS-specific code
    var root = NSBundle.MainBundle.BundlePath;
#elif WINDOWS
    // Windows-specific code
    if (IsPackagedApp.Value)
        return Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#endif
```

### Service-Oriented Architecture

The CLI tool demonstrates clean service architecture:

```csharp
// Services are injected and focused on single responsibilities
public class PackageService
{
    public Task<PackageIdentity> GetPackageIdentity(string packagePath) { }
    public Task<bool> IsPackageInstalled(string identity) { }
    public Task InstallPackage(string packagePath) { }
}
```

### Command Pattern for CLI

CLI commands follow Spectre.Console command pattern:

```csharp
public class WindowsAppLaunchCommand : AsyncCommand<WindowsAppLaunchCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--app")]
        public string? AppPath { get; set; }
        
        [CommandOption("--identity")]
        public string? Identity { get; set; }
    }
}
```

### MVVM Pattern for Visual Runners

Visual runners use MVVM with observable collections:

```csharp
public class HomeViewModel : AbstractBaseViewModel
{
    public ObservableCollection<TestAssemblyViewModel> TestAssemblies { get; } = new();
    public ICommand RunEverythingCommand { get; }
    
    // Async command execution with busy state management
    async Task RunEverythingAsync() { /* ... */ }
}
```

## Testing Patterns

### Device Test Structure

Device tests inherit from framework-specific base classes:

```csharp
// For Xunit device tests
public class DeviceSpecificTest : IClassFixture<DeviceFixture>
{
    [Fact]
    public async Task TestDeviceFeature() { }
}

// For NUnit device tests  
[TestFixture]
public class DeviceSpecificTest
{
    [Test]
    public async Task TestDeviceFeature() { }
}
```

### CLI Tool Testing

CLI tests use Spectre.Console.Testing:

```csharp
public class CommandTests
{
    private readonly CommandAppTester _app;
    
    public CommandTests()
    {
        _app = new CommandAppTester();
        _app.Configure(config => {
            config.AddBranch("windows", windows => {
                windows.AddCommand<WindowsAppLaunchCommand>("launch");
            });
        });
    }
    
    [Fact] 
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        var result = _app.Run("windows", "launch", "--output", "json");
        Assert.True(TestHelpers.IsValidJson(result.Output));
    }
}
```

## Build System Conventions

### MSBuild Configuration
- **Artifacts output**: Uses `UseArtifactsOutput` for modern build output structure
- **Central Package Management**: Package versions centralized in `Directory.Packages.props`
- **Platform targeting**: Conditional framework targeting based on build OS

### Platform-Specific Compilation

```xml
<ItemGroup>
    <Compile Remove="Platforms\**" />
    <Compile Include="Platforms\All\**" />
    <Compile Include="Platforms\Android\**" Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'" />
    <Compile Include="Platforms\iOS\**" Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'" />
    <!-- etc. for other platforms -->
</ItemGroup>
```

## Development Guidelines

### Adding New Test Framework Support

1. Create new project: `DeviceRunners.VisualRunners.{Framework}`
2. Implement framework-specific discoverers and runners
3. Reference from main visual runner project
4. Add corresponding XHarness integration if needed
5. Update sample applications with example tests

### Adding New Platform Support

1. Add platform target to relevant `.csproj` files
2. Create platform-specific folder under `Platforms/{Platform}/`
3. Implement platform-specific services and utilities
4. Update conditional compilation in shared code
5. Add platform-specific tests

### CLI Tool Extensions

1. Commands are organized under feature branches (e.g., `windows`, `network`)
2. Each command has its own settings class inheriting `CommandSettings`
3. Support multiple output formats: console (default), JSON, XML, text
4. Provide rich console output for humans, clean structured output for automation

### Visual Runner Extensions

1. ViewModels follow MVVM pattern with `AbstractBaseViewModel` base
2. Use `ObservableCollection` for dynamic data binding
3. Commands implement `ICommand` with async execution patterns
4. Support both test discovery and execution phases

## Key Interfaces and Abstractions

### Core Testing Interfaces
```csharp
public interface ITestDiscoverer
{
    Task<IEnumerable<TestAssemblyInfo>> DiscoverTestsAsync(IEnumerable<string> sources);
}

public interface ITestRunner  
{
    Task<TestRunSummary> RunTestsAsync(IEnumerable<TestCase> testCases);
}

public interface IResultChannelManager
{
    Task SendResultsAsync(TestResult result);
}
```

### Platform Abstractions
```csharp
public interface IAppTerminator
{
    Task TerminateAsync();
}

public interface IDiagnosticsManager
{
    Task<DiagnosticData> CollectDiagnosticsAsync();
}
```

## Common Scenarios for Copilot Assistance

### When Adding Test Framework Support
- Implement `ITestDiscoverer` and `ITestRunner` for the framework
- Create framework-specific result adapters
- Add Visual and XHarness runner implementations
- Include sample test projects

### When Adding Platform Features  
- Use appropriate platform folder structure
- Implement platform-specific services in Core library
- Add conditional compilation directives
- Create platform-specific test scenarios

### When Extending CLI Tool
- Follow Spectre.Console command patterns
- Support multiple output formats consistently  
- Add comprehensive unit tests with mocked services
- Update README documentation

### When Working with MAUI Integration
- Use MAUI service registration patterns
- Implement platform-specific handlers where needed
- Follow MAUI lifecycle management
- Test across all target platforms

## Historical Context

This project consolidates and modernizes several earlier testing solutions:
- **xunit/devices.xunit**: Migrated to .NET MAUI with separated UI components
- **xunit/uitest.xunit**: Migrated to .NET MAUI  
- **nunit/nunit.xamarin**: Migrated to .NET MAUI with individual test support
- **dotnet/maui**: Temporary hosting during migration

The architecture reflects lessons learned from these migrations, emphasizing modularity, cross-platform support, and separation of concerns between test execution and user interfaces.