# DeviceRunners Project Templates

Project templates for creating device test projects using the [DeviceRunners](https://github.com/mattleibow/DeviceRunners) visual test runner.

## Installation

### Install the Templates

```bash
dotnet new install DeviceRunners.Templates
```

This installs two templates:

| Template | Short Name | Description |
|----------|-----------|-------------|
| DeviceRunners Device Tests | `devicerunners` | .NET MAUI app for testing on Android, iOS, macOS, and Windows |
| DeviceRunners Browser Tests | `devicerunners-browser` | Blazor WebAssembly app for testing in the browser |

## Quick Start — Device Tests (MAUI)

### 1. Create a Device Tests Project

```bash
dotnet new devicerunners -n MyApp.DeviceTests
```

This creates a .NET MAUI app configured as a device test runner with xUnit.

**Template options:**

| Option              | Description                          | Default                            |
|---------------------|--------------------------------------|------------------------------------|
| `-n`, `--name`      | Project name                         | `DeviceTests`                      |
| `--Framework`       | Target .NET version (`net9.0`/`net10.0`) | `net9.0`                       |
| `--applicationId`   | Application identifier               | `com.companyname.devicetests`      |

### 2. Write Your Tests

Add xUnit test classes to the project. They'll be discovered automatically:

```csharp
public class MyDeviceTests
{
    [Fact]
    public void TestDeviceFeature()
    {
        // This test runs on the actual device
        Assert.True(true);
    }
}
```

### 3. Run Tests Interactively

Build and deploy the app to a device or emulator. The visual test runner UI lets you browse and run tests interactively:

```bash
# Run on Android emulator
dotnet build -t:Run -f net9.0-android

# Run on iOS simulator
dotnet build -t:Run -f net9.0-ios

# Run on Mac Catalyst
dotnet build -t:Run -f net9.0-maccatalyst

# Run on Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

### 4. Run Tests in CI/CD

**Recommended: `dotnet test`**

The template includes `DeviceRunners.Testing.Targets` which hooks into the standard `dotnet test` workflow:

```bash
# Run on each platform — that's it!
dotnet test MyApp.DeviceTests.csproj -f net9.0-android
dotnet test MyApp.DeviceTests.csproj -f net9.0-ios
dotnet test MyApp.DeviceTests.csproj -f net9.0-maccatalyst
dotnet test MyApp.DeviceTests.csproj -f net9.0-windows10.0.19041.0
```

This automatically builds, deploys, launches the app, collects results via TCP, and produces a TRX file.

**Alternative: DeviceRunners CLI**

For more control, use the CLI tool directly:

```bash
# Install the CLI tool
dotnet tool install --global DeviceRunners.Cli

# macOS: publish and run
dotnet publish -f net9.0-maccatalyst -c release
device-runners macos test --app path/to/MyApp.app --results-directory results --logger trx

# Windows (unpackaged EXE):
dotnet publish -f net9.0-windows10.0.19041.0 -c release -p:WindowsPackageType=None --output publish
device-runners windows test --app publish/MyApp.exe --results-directory results --logger trx

# Windows (MSIX):
dotnet publish -f net9.0-windows10.0.19041.0 -c release
device-runners windows test --app path/to/app.msix --results-directory results --logger trx
```

## Quick Start — Browser Tests (Blazor WASM)

### 1. Create a Browser Tests Project

```bash
dotnet new devicerunners-browser -n MyApp.BrowserTests
```

This creates a Blazor WebAssembly app configured as a browser test runner with xUnit.

**Template options:**

| Option              | Description                          | Default          |
|---------------------|--------------------------------------|------------------|
| `-n`, `--name`      | Project name                         | `BrowserTests`   |
| `--Framework`       | Target .NET version (`net9.0`/`net10.0`) | `net9.0`     |

### 2. Write Your Tests

```csharp
public class MyBrowserTests
{
    [Fact]
    public void TestInBrowser()
    {
        // This test runs in the browser via WebAssembly
        Assert.True(true);
    }
}
```

### 3. Run Tests Interactively

```bash
dotnet run
```

Open the URL in your browser to see the visual test runner UI.

### 4. Run Tests in CI/CD

```bash
# Using dotnet test (recommended — launches headless Chrome automatically)
dotnet test MyApp.BrowserTests.csproj

# Or using the CLI tool
dotnet tool install --global DeviceRunners.Cli
dotnet publish -c release
device-runners wasm test --app bin/release/net9.0/publish/wwwroot
```

## How It Works

### Device Tests (MAUI)

The template creates a .NET MAUI app that uses the DeviceRunners visual test runner:

- **Interactive mode (default):** Shows a UI for browsing and running tests on the device.
- **CI/CD mode:** When launched by `dotnet test` or the DeviceRunners CLI, the app detects configuration (via environment variables or CLI arguments) and automatically runs all tests, streaming results over TCP.

The `AddCliConfiguration()` method in `MauiProgram.cs` handles this detection — it's a no-op when running interactively.

### Browser Tests (Blazor WASM)

The template creates a Blazor WebAssembly app:

- **Interactive mode (default):** Shows a browser-based UI for browsing and running tests.
- **CI/CD mode:** When launched by `dotnet test` or the CLI, a headless Chrome instance navigates to the app with `?device-runners-autorun=1` in the URL, test results are captured from the browser console.

## Configuration

When using `dotnet test`, configure via MSBuild properties:

| Property | Default | Description |
|----------|---------|-------------|
| `DeviceRunnersPort` | `16384` | TCP port for test result collection |
| `DeviceRunnersConnectionTimeout` | `120` | Seconds to wait for the app to connect |
| `DeviceRunnersDataTimeout` | `30` | Seconds of silence before assuming the run ended |
| `DeviceRunnersDevice` | _(auto)_ | Target device ID (Android emulator serial, iOS simulator UDID) |
| `DeviceRunnersWasmTimeout` | `300` | WASM: test execution timeout in seconds |

Example:

```bash
dotnet test MyApp.csproj -f net9.0-ios -p:DeviceRunnersDevice=ABCD1234
```

## Adding Test Assemblies from Other Projects

To run tests from other projects, add a project reference and register the assembly:

**MAUI (`MauiProgram.cs`):**

```csharp
builder.UseVisualTestRunner(conf => conf
    .AddCliConfiguration()
    .AddConsoleResultChannel()
    .AddTestAssembly(typeof(MauiProgram).Assembly)
    .AddTestAssemblies(typeof(MyLibrary.Tests.SomeTestClass).Assembly)
    .AddXunit());
```

**Blazor WASM (`Program.cs`):**

```csharp
builder.UseVisualTestRunner(conf => conf
    .AddXunit(useReflection: true)
    .AddTestAssembly(typeof(Program).Assembly)
    .AddTestAssemblies(typeof(MyLibrary.Tests.SomeTestClass).Assembly)
    .AddConsoleResultChannel());
```

Don't forget to add a `TrimmerRootAssembly` entry in the `.csproj` to prevent the linker from stripping test classes:

```xml
<ItemGroup>
    <TrimmerRootAssembly Include="MyLibrary.Tests" RootMode="all" />
</ItemGroup>
```
