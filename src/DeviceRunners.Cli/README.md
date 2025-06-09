# DeviceRunners CLI Tool

A cross-platform .NET CLI tool that provides testing utilities for device applications, converting the original PowerShell scripts into a unified command-line interface using Spectre.Console.

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

## Global Options

All commands support a global `--output` option for structured results that can be consumed by automation tools:

**Output Formats:**
- `--output json` - Returns results as JSON
- `--output xml` - Returns results as XML  
- `--output text` - Returns results as key=value pairs
- (no --output) - Shows rich console output for human users (default)

**Examples:**
```bash
# For human users - shows rich colored output with progress
device-runners windows cert install --publisher "CN=Test"

# For automation - clean JSON output  
device-runners windows cert install --publisher "CN=Test" --output json

# For automation - simple text format
device-runners windows cert install --publisher "CN=Test" --output text
```

When `--output` is specified, verbose console logs are suppressed and only the structured result is returned, making it ideal for CI/CD pipelines and automation scripts.

## Commands

### Windows Commands

All Windows commands support both `--app` (MSIX package path) and `--identity` (application name) parameters for flexibility, unless otherwise specified.

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

#### Application Management

##### Install Application (`windows install`)

Install an MSIX application package with automatic certificate handling and dependency installation.

```bash
# Install with automatic certificate detection
device-runners windows install --app "path/to/app.msix"

# Install with specific certificate
device-runners windows install --app "path/to/app.msix" --certificate "path/to/cert.pfx"
```

**Options:**
- `--app` - Path to the MSIX application package (required)
- `--certificate` - Path to the certificate file (optional, auto-detected if not provided)

**Features:**
- Automatically installs dependencies from `Dependencies/{arch}` folder
- Tracks if certificate was auto-installed (`CertificateAutoInstalled` in output)
- Handles certificate installation if needed

**Platform Support:** Windows only

##### Uninstall Application (`windows uninstall`)

Uninstall an application by package path or application identity, with optional certificate cleanup.

```bash
# Uninstall by MSIX package path
device-runners windows uninstall --app "path/to/app.msix"

# Uninstall by application identity/name
device-runners windows uninstall --identity "MyApplication"

# Uninstall application and remove certificate
device-runners windows uninstall --identity "MyApplication" --certificate-fingerprint "ABCD1234..."
```

**Options:**
- `--app` - Path to the MSIX application package
- `--identity` - Application identity/name to uninstall
- `--certificate-fingerprint` - Certificate fingerprint to remove after uninstalling package

**Note:** Either `--app` or `--identity` must be provided.

**Platform Support:** Windows only

##### Launch Application (`windows launch`)

Launch an installed application with optional arguments.

```bash
# Launch by MSIX package path
device-runners windows launch --app "path/to/app.msix"

# Launch by application identity
device-runners windows launch --identity "MyApplication"

# Launch with custom arguments
device-runners windows launch --identity "MyApplication" --args "test-arguments"
```

**Options:**
- `--app` - Path to the MSIX application package
- `--identity` - Application identity/name to launch
- `--args` - Launch arguments to pass to the application

**Note:** Either `--app` or `--identity` must be provided.

**Platform Support:** Windows only

#### Test Execution (`windows test`)

Install and start a test application with TCP listener for receiving test results. Performs complete test workflow with automatic cleanup.

```bash
# Basic test execution
device-runners windows test --app "path/to/app.msix"

# With custom output directory
device-runners windows test --app "path/to/app.msix" --results-directory "test-results"

# With specific certificate
device-runners windows test --app "path/to/app.msix" --certificate "path/to/cert.pfx"
```

**Options:**
- `--app` - Path to the MSIX application package (required)
- `--certificate` - Path to the certificate file (optional, auto-detected if not provided)
- `--results-directory` - Results directory for test outputs (default: "artifacts")

**Workflow:**
1. **Preparation Phase:**
   - Determines certificate path and fingerprint
   - Extracts app identity from MSIX package
   - Uninstalls app if already installed
   - Installs certificate if not present (tracks for cleanup)
   - Installs dependencies from `Dependencies/{arch}` folder
   - Installs the main application

2. **Execution Phase:**
   - Launches the application
   - Starts TCP listener on port 16384 for test results
   - Waits for test completion (10-minute timeout)
   - Analyzes test results

3. **Cleanup Phase:**
   - Uninstalls the application
   - Removes auto-installed certificate (if applicable)

**Platform Support:** Windows only

### Network Commands

#### TCP Port Listener (`listen`)

Start a TCP port listener for receiving test results.

```bash
# Basic TCP listener
device-runners listen --port 16384

# With results file and non-interactive mode
device-runners listen --port 16384 --results-file results.txt --non-interactive

# With custom timeouts for non-interactive mode  
device-runners listen --port 16384 --non-interactive --connection-timeout 60 --data-timeout 45

# For automation - JSON output
device-runners listen --port 16384 --non-interactive --output json
```

**Options:**
- `--port` - TCP port to listen on (default: 16384)
- `--results-file` - File path to save received data
- `--non-interactive` - Run in non-interactive mode with timeout
- `--connection-timeout` - Connection timeout in seconds (default: 30, non-interactive mode only)
- `--data-timeout` - Data timeout in seconds (default: 30, non-interactive mode only)

**Platform Support:** Cross-platform

## Architecture

### Services

The CLI tool is built with a clean service-oriented architecture:

#### PackageService
Handles MSIX package operations:
- `GetPackageIdentity()` - Extract app identity from MSIX packages
- `IsPackageInstalled()` - Check if package is installed
- `InstallPackage()` - Install MSIX packages
- `UninstallPackage()` - Uninstall packages
- `LaunchApp()` - Launch installed applications
- `GetDependencies()` - Find dependency packages
- `GetCertificateFromMsix()` - Locate certificate files

#### CertificateService
Manages certificates for code signing:
- `CreateSelfSignedCertificate()` - Generate self-signed certificates
- `RemoveCertificate()` - Remove certificates by thumbprint
- `InstallCertificate()` - Install certificates (with native C# fallback)
- `UninstallCertificate()` - Remove installed certificates
- `IsCertificateInstalled()` - Check certificate presence
- `GetCertificateFingerprint()` - Get certificate thumbprint

#### NetworkService
Provides network functionality:
- TCP port listening for test results
- Cross-platform network operations

#### PowerShellService
Centralized PowerShell execution:
- Handles Windows-specific operations requiring PowerShell
- Provides consistent error handling and elevation support

## Usage Examples

```bash
# Install as global tool
dotnet tool install --global DeviceRunners.Cli

# Certificate operations (Windows only)
device-runners windows cert install --publisher "CN=MyCompany"
device-runners windows cert uninstall --fingerprint "ABCD1234..."

# Application management (Windows only)
device-runners windows install --app app.msix
device-runners windows launch --identity "MyApp" --args "test-mode"
device-runners windows uninstall --identity "MyApp" --certificate-fingerprint "ABCD1234..."

# Network operations (cross-platform)  
device-runners listen --port 16384 --results-file results.txt

# Test execution (Windows only)
device-runners windows test --app app.msix
```

## Original PowerShell Scripts

This CLI tool replaces the following PowerShell scripts with enhanced functionality:

- `New-Certificate.ps1` → `windows cert install` command
- `Remove-Certificate.ps1` → `windows cert uninstall` command  
- `New-PortListener.ps1` → `listen` command
- `Start-Tests.ps1` → `windows test` command

**Enhancements over PowerShell scripts:**
- **Unified interface** - Single tool replaces 4 separate scripts
- **Cross-platform support** - Network tools work on all platforms
- **Better error handling** - Consistent error reporting and validation
- **Rich output** - Colored console output with structured data options
- **Additional commands** - Separate install/uninstall/launch commands
- **Native C# implementations** - Reduced PowerShell dependencies where possible
- **Full test coverage** - Comprehensive test suite

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

- **Spectre.Console.Cli** - Command line interface framework with rich console output
- **System.Security.Cryptography.X509Certificates** - Certificate management
- **Built-in .NET libraries** - Networking, file I/O, and compression