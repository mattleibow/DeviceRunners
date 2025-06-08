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

### Certificate Management

#### Create Certificate (`cert-create`)

Generate and install a self-signed certificate for MSIX packages.

```bash
# Create certificate with publisher identity
device-runners cert-create --publisher "CN=MyCompany"

# Create certificate from manifest file
device-runners cert-create --manifest "path/to/Package.appxmanifest"

# Create certificate from project directory
device-runners cert-create --project "path/to/project"
```

**Options:**
- `--publisher` - Publisher identity for the certificate
- `--manifest` - Path to Package.appxmanifest file
- `--project` - Path to project directory

**Platform Support:** Windows only

#### Remove Certificate (`cert-remove`)

Remove a certificate by its fingerprint.

```bash
device-runners cert-remove --fingerprint "ABCD1234567890..."
```

**Options:**
- `--fingerprint` - Certificate fingerprint to remove (required)

**Platform Support:** Windows only

### Network Tools

#### Port Listener (`port-listen`)

Start a TCP port listener for receiving test results.

```bash
# Listen on default port (16384)
device-runners port-listen

# Listen on custom port
device-runners port-listen --port 8080

# Save received data to file
device-runners port-listen --port 16384 --output results.txt

# Run in non-interactive mode (terminate after first connection)
device-runners port-listen --port 16384 --non-interactive
```

**Options:**
- `--port` - TCP port to listen on (default: 16384)
- `--output` - Path to save received data
- `--non-interactive` - Run in non-interactive mode

### Test Execution

#### Test Starter (`test-start`)

Install and start a test application with various testing modes.

```bash
# Basic app installation and start
device-runners test-start --app "path/to/app.msix"

# With custom certificate
device-runners test-start --app "path/to/app.msix" --certificate "path/to/cert.cer"

# With XHarness testing mode
device-runners test-start --app "path/to/app.msix" --testing-mode XHarness

# With non-interactive visual testing
device-runners test-start --app "path/to/app.msix" --testing-mode NonInteractiveVisual --output-directory "test-results"
```

**Options:**
- `--app` - Path to the MSIX application package (required)
- `--certificate` - Path to the certificate file (optional, auto-detected if not provided)
- `--output-directory` - Output directory for test results (default: "artifacts")
- `--testing-mode` - Testing mode: `XHarness`, `NonInteractiveVisual`, or `None`

**Platform Support:** Windows only

## Testing Modes

- **XHarness**: Launches the app with XHarness test runner arguments
- **NonInteractiveVisual**: Starts the app and listens for test results via TCP on port 16384
- **None**: Simple app launch without special testing configuration

## Original PowerShell Scripts

This CLI tool replaces the following PowerShell scripts:

- `New-Certificate.ps1` → `cert-create` command
- `Remove-Certificate.ps1` → `cert-remove` command
- `New-PortListener.ps1` → `port-listen` command
- `Start-Tests.ps1` → `test-start` command

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