# DeviceRunners CLI Test Workflow


This document provides detailed information about how the DeviceRunners CLI tool executes tests across different platforms. Understanding this workflow can help with troubleshooting and advanced configuration scenarios.

## General Test Workflow

All platforms follow a similar three-phase approach:

### 1. Preparation Phase
The CLI tool prepares the environment and application for testing:
- Validates input parameters and file paths
- Handles platform-specific requirements
- Manages application installation and dependencies
- Sets up necessary certificates or permissions

### 2. Execution Phase
The actual test execution:
- Launches the target application
- Starts TCP listener for test result communication
- Monitors test progress with configurable timeouts
- Captures platform-specific logs and diagnostics

### 3. Cleanup Phase
Post-test cleanup and result processing:
- Saves test results and logs
- Uninstalls temporary applications
- Cleans up certificates or permissions (if auto-installed)
- Generates final test reports

## Platform-Specific Workflows

### Windows Test Workflow

#### Packaged Applications (.msix)

**Preparation:**
1. Determines certificate path and fingerprint from MSIX package
2. Extracts application identity from the package
3. Uninstalls the application if already installed
4. Checks if certificate is installed, installs if needed (tracks for cleanup)
5. Installs dependencies from `Dependencies/{arch}` folder automatically
6. Installs the main application package

**Execution:**
1. Launches the application
2. Starts TCP listener on specified port (default: 16384)
3. Waits for test completion with configurable timeouts
4. Captures and analyzes test results in real-time

**Cleanup:**
1. Uninstalls the application
2. Removes auto-installed certificate (if the tool installed it)
3. Preserves test results for analysis

#### Unpackaged Applications (.exe)

**Preparation:**
1. Validates that the executable file exists and is accessible
2. Verifies the application can be launched

**Execution:**
1. Starts the application directly as a process
2. Starts TCP listener on specified port for test results
3. Monitors the application process and test results
4. Handles test completion and result analysis

**Cleanup:**
1. Terminates the application process if still running
2. Preserves test results for analysis

### Android Test Workflow

**Preparation:**
1. Clears logcat to ensure clean logs
2. Validates the target device (if specified)
3. Installs the APK package (if `--app` is provided)
4. Determines the package name from APK or uses provided package name
5. Validates that the app is properly installed

**Execution:**
1. Starts the application with the specified activity
2. Begins TCP listener on specified port for test results
3. Waits for test completion with configurable timeouts
4. Captures all logcat output during test execution

**Cleanup:**
1. Saves logcat output to `logcat.txt` in the results directory
2. Preserves test results and logs for analysis

### macOS Test Workflow

**Preparation:**
1. Determines the application identifier from the .app bundle
2. Validates the app bundle structure and metadata
3. Uninstalls the application if already installed (ensures clean state)
4. Installs the application bundle to the system

**Execution:**
1. Launches the application
2. Starts TCP listener on specified port for test results
3. Waits for test completion with configurable timeouts
4. Captures and analyzes test results in real-time

**Cleanup:**
1. Uninstalls the application to clean up the system
2. Preserves test results for analysis

### iOS Test Workflow

**Preparation:**
1. Validates the .app bundle and determines bundle identifier
2. Identifies target simulator (uses booted simulator if `--device` not specified)
3. Installs the application to the simulator

**Execution:**
1. Launches the application on the simulator
2. Starts TCP listener on specified port for test results
3. Waits for test completion with configurable timeouts
4. Captures and analyzes test results in real-time

**Cleanup:**
1. Preserves test results for analysis

## Test Result Communication

### TCP Protocol
The CLI tool uses TCP communication to receive test results from the running application:

1. **Listener Setup**: TCP listener starts on specified port (default: 16384)
2. **Connection Wait**: Waits for application to connect within connection timeout
3. **Data Reception**: Receives test result data with data timeout between messages
4. **Result Processing**: Parses and analyzes received test results
5. **Completion**: Closes connection when tests complete or timeout occurs

### Result File Generation
Test results are saved in the specified results directory:
- **TestResults.xml**: Main test result file in standardized format
- **Platform-specific logs**: Additional diagnostic information (e.g., logcat.txt for Android)

## Timeout Configuration

### Connection Timeout
- **Default**: 120 seconds
- **Purpose**: Maximum time to wait for initial application connection
- **Configurable**: `--connection-timeout` parameter

### Data Timeout  
- **Default**: 30 seconds
- **Purpose**: Maximum time between data transmissions
- **Configurable**: `--data-timeout` parameter
- **Behavior**: Resets on each data received

## Error Handling

### Pre-Execution Validation
- **File Validation**: Checks for file existence, format, and accessibility
- **Platform Validation**: Ensures required tools and dependencies are available
- **Permission Validation**: Verifies necessary permissions for installation and execution
- **Network Validation**: Checks port availability for TCP communication

### Runtime Error Recovery
- **Installation Failures**: Provides detailed error messages with context
- **Launch Failures**: Attempts to diagnose and report launch issues
- **Network Failures**: Handles connection timeouts and communication errors
- **Resource Cleanup**: Ensures cleanup occurs even on failure

### Logging and Diagnostics
- **Verbose Logging**: Detailed operation logs for troubleshooting
- **Progress Indicators**: Real-time feedback on operation status
- **Error Context**: Contextual information for error diagnosis
- **Exit Codes**: Standardized exit codes for automation integration

## Advanced Configuration

### Custom Network Configuration
```bash
device-runners [platform] test \
  --app path/to/app \
  --port 8080 \
  --connection-timeout 60 \
  --data-timeout 45 \
  --results-directory custom-results
```

### Platform-Specific Options

#### Windows-Specific
- **Certificate Management**: Automatic detection and installation
- **Dependency Handling**: Automatic installation of required dependencies
- **Package vs. Executable**: Automatic detection and appropriate handling

#### Android-Specific  
- **Device Targeting**: Specify target device or emulator
- **Activity Selection**: Custom main activity specification
- **Logcat Integration**: Automatic log capture and preservation

#### macOS-Specific
- **Bundle Validation**: App bundle structure and metadata validation
- **System Integration**: Proper installation and uninstallation handling

## Troubleshooting Common Issues

### Connection Timeouts
- Increase connection timeout for slow-starting applications
- Verify network port is not blocked by firewall
- Check application actually attempts to connect to TCP port

### Installation Failures
- **Windows**: Verify certificate installation and dependency availability
- **Android**: Check device connectivity and storage space
- **macOS**: Verify app bundle format and system permissions

### Test Result Issues
- Verify application implements proper test result reporting
- Check TCP communication implementation in test application
- Review timeout settings for long-running tests

This workflow documentation provides the technical details needed for advanced configuration and troubleshooting of the DeviceRunners CLI tool.
