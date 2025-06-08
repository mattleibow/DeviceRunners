# DeviceRunners CLI Tool

A dotnet CLI tool that provides testing utilities for device applications, converting the original PowerShell scripts into a coherent command-line interface.

## Installation

### As a Global Tool

```bash
dotnet pack
dotnet tool install --global --add-source ./bin/Release DeviceRunners.Cli
```

### Direct Execution

```bash
dotnet run -- [command] [options]
```

## Commands

### Windows Commands

#### Certificate Management

##### Install Certificate (`windows cert install`)

Generate and install a self-signed certificate for MSIX packages.

```bash
# Create certificate with publisher identity
device-runners windows cert install --publisher "CN=MyCompany"

# Create certificate from manifest file
device-runners windows cert install --manifest "path/to/Package.appxmanifest"

# Create certificate from project directory  
device-runners windows cert install --project "path/to/project"
```

**Options:**
- `--publisher` - Publisher identity for the certificate
- `--manifest` - Path to Package.appxmanifest file
- `--project` - Path to project directory

**Platform Support:** Windows only

##### Uninstall Certificate (`windows cert uninstall`)

Remove a certificate by its fingerprint.

```bash
device-runners windows cert uninstall --fingerprint "ABCD1234..."
```

**Options:**
- `--fingerprint` - Certificate fingerprint to remove

**Platform Support:** Windows only

#### Test Execution (`windows test`)

Install and start a test application.

```bash
# Basic test execution
device-runners windows test --app "path/to/app.msix"

# With XHarness testing mode (includes folder watcher for real-time log streaming)
device-runners windows test --app "path/to/app.msix" --testing-mode XHarness

# With NonInteractiveVisual testing mode (TCP listener for results)
device-runners windows test --app "path/to/app.msix" --testing-mode NonInteractiveVisual
```

**Options:**
- `--app` - Path to the MSIX application package (required)
- `--certificate` - Path to the certificate file (optional, auto-detected if not provided)
- `--output-directory` - Output directory for test results (default: "artifacts")
- `--testing-mode` - Testing mode: `XHarness`, `NonInteractiveVisual`, or `None`

**Testing Modes:**
- **XHarness**: Launches the app with XHarness test runner arguments and monitors test-output-*.log files in real-time
- **NonInteractiveVisual**: Starts a TCP listener on port 16384 to receive test results
- **None**: Basic app launch without special test handling

**Platform Support:** Windows only

### TCP Commands

#### Port Listener (`tcp listener start`)

Start a TCP port listener for receiving test results.

```bash
# Basic TCP listener
device-runners tcp listener start --port 16384

# With output file and non-interactive mode
device-runners tcp listener start --port 16384 --output results.txt --non-interactive
```

**Options:**
- `--port` - TCP port to listen on (default: 16384)
- `--output` - Output file path for received data
- `--non-interactive` - Run in non-interactive mode with timeout

**Platform Support:** Cross-platform

## Usage Examples

```bash
# Install as global tool
dotnet tool install --global DeviceRunners.Cli

# Certificate operations (Windows only)
device-runners windows cert install --publisher "CN=MyCompany"
device-runners windows cert uninstall --fingerprint "ABCD1234..."

# Network operations (cross-platform)  
device-runners tcp listener start --port 16384 --output results.txt

# Test execution (Windows only)
device-runners windows test --app app.msix --testing-mode XHarness
## Original PowerShell Scripts

This CLI tool replaces the following PowerShell scripts:

- `New-Certificate.ps1` → `windows cert install` command
- `Remove-Certificate.ps1` → `windows cert uninstall` command
- `New-PortListener.ps1` → `tcp listener start` command
- `Start-Tests.ps1` → `windows test` command

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Packaging

```bash
dotnet pack
```

## Platform Requirements

- .NET 9.0 or later
- Windows (for certificate and app management commands)
- Cross-platform support for network tools

## Dependencies

- Spectre.Console.Cli - Command line interface framework
- System.Security.Cryptography.X509Certificates - Certificate management
- Built-in .NET networking libraries