# DeviceRunners Project Templates

Project templates for creating device test projects using the [DeviceRunners](https://github.com/mattleibow/DeviceRunners) visual test runner with TCP result streaming.

## Installation

### Install the Template

```bash
dotnet new install DeviceRunners.Templates
```

### Install the CLI Tool

The DeviceRunners CLI tool (`device-runners`) is used to run device tests from the command line, listen for TCP test results, and manage app deployment on Windows.

**As a local tool (recommended for projects):**

```bash
dotnet new tool-manifest   # if you don't already have one
dotnet tool install DeviceRunners.Cli --local
```

**As a global tool:**

```bash
dotnet tool install --global DeviceRunners.Cli
```

## Quick Start

### 1. Create a Device Tests Project

```bash
dotnet new devicerunners -n MyApp.DeviceTests
```

This creates a .NET MAUI app configured as a device test runner with xUnit and TCP result streaming.

**Template options:**

| Option              | Description                          | Default                            |
|---------------------|--------------------------------------|------------------------------------|
| `-n`, `--name`      | Project name                         | `DeviceTests`                      |
| `--Framework`       | Target .NET version (`net9.0`/`net10.0`) | `net9.0`                       |
| `--applicationId`   | Application identifier               | `com.companyname.devicetests`      |

**Examples:**

```bash
# .NET 10.0 project with custom app ID
dotnet new devicerunners -n MyApp.DeviceTests --Framework net10.0 --applicationId com.mycompany.myapp.tests
```

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

### 4. Run Tests in CI/CD (TCP Mode)

For automated testing, the CLI tool listens for TCP results while the app runs tests in headless mode.

**macOS / Mac Catalyst:**

```bash
# Terminal 1: Start the TCP listener
dotnet device-runners listen --port 16384 --non-interactive --results-file results.txt

# Terminal 2: Run the app with environment variables to enable auto-run
DEVICE_RUNNERS_AUTORUN=1 DEVICE_RUNNERS_PORT=16384 dotnet build -t:Run -f net9.0-maccatalyst
```

**Windows:**

The CLI tool can handle the full test workflow on Windows (install, launch, listen, cleanup):

```bash
# Build the MSIX package first
dotnet publish -f net9.0-windows10.0.19041.0

# Run the full test workflow
dotnet device-runners windows test --app path/to/app.msix
```

**Android (using adb port forwarding):**

```bash
# Forward the TCP port to the emulator/device
adb reverse tcp:16384 tcp:16384

# Terminal 1: Start the TCP listener
dotnet device-runners listen --port 16384 --non-interactive --results-file results.txt

# Terminal 2: Run the app
dotnet build -t:Run -f net9.0-android
```

## How It Works

The template creates a .NET MAUI app that uses the DeviceRunners visual test runner:

- **Interactive mode (default):** Shows a UI for browsing and running tests on the device.
- **CI/CD mode:** When the `DEVICE_RUNNERS_AUTORUN` environment variable is set, the app automatically runs all tests and streams results over TCP to the CLI listener.

### Environment Variables

| Variable                    | Description                                        | Default              |
|-----------------------------|----------------------------------------------------|----------------------|
| `DEVICE_RUNNERS_AUTORUN`    | Set to any value to enable headless auto-run mode  | _(unset)_            |
| `DEVICE_RUNNERS_PORT`       | TCP port to stream results to                      | `16384`              |
| `DEVICE_RUNNERS_HOST_NAMES` | Semicolon-separated host names to try              | `localhost;10.0.2.2` |

### CLI Commands

```bash
# Listen for TCP test results (cross-platform)
dotnet device-runners listen --port 16384 --non-interactive --results-file results.txt

# Windows: Full test workflow (install, launch, listen, cleanup)
dotnet device-runners windows test --app path/to/app.msix

# Windows: Individual commands
dotnet device-runners windows install --app path/to/app.msix
dotnet device-runners windows launch --app path/to/app.msix
dotnet device-runners windows uninstall --app path/to/app.msix

# Windows: Certificate management
dotnet device-runners windows cert install --publisher "CN=MyCompany"
dotnet device-runners windows cert uninstall --fingerprint "ABCD1234..."
```

## Project Structure

After running `dotnet new devicerunners -n MyApp.DeviceTests`, you get:

```
MyApp.DeviceTests/
├── MauiProgram.cs              # Test runner configuration
├── SampleTests.cs              # Example test class
├── Usings.cs                   # Global usings
├── MyApp.DeviceTests.csproj    # Project file
├── Properties/
│   └── launchSettings.json
├── Platforms/
│   ├── Android/                # Android platform files
│   ├── iOS/                    # iOS platform files
│   ├── MacCatalyst/            # macOS Catalyst platform files
│   └── Windows/                # Windows platform files
└── Resources/
    ├── AppIcon/                # App icons
    ├── Images/                 # Image assets
    └── Splash/                 # Splash screen
```

## Adding Test Assemblies from Other Projects

To run tests from other projects (e.g., a shared test library), add a project reference and register the assembly in `MauiProgram.cs`:

```csharp
builder.UseVisualTestRunner(conf => conf
    .AddEnvironmentVariables()
    .AddConsoleResultChannel()
    .AddTestAssembly(typeof(MauiProgram).Assembly)
    .AddTestAssemblies(typeof(MyLibrary.Tests.SomeTestClass).Assembly)
    .AddXunit());
```

Don't forget to add a `TrimmerRootAssembly` entry in the `.csproj` to prevent the linker from stripping test classes:

```xml
<ItemGroup>
    <TrimmerRootAssembly Include="MyLibrary.Tests" RootMode="all" />
</ItemGroup>
```
