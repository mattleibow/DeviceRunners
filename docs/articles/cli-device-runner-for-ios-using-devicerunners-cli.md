# iOS Testing with DeviceRunners CLI

> [!NOTE]
> This documentation was partially generated using AI and may contain mistakes or be missing information. Please verify commands and procedures before use, and report any issues or improvements needed.

This guide covers testing iOS Simulator applications using the DeviceRunners CLI tool.

## Running Tests

The DeviceRunners CLI tool provides a comprehensive test workflow for iOS Simulator applications that handles installation, execution, and cleanup automatically.

1. Build the app bundle for testing:  
   ```
   dotnet build <path/to/app.csproj> -f net9.0-ios -r <runtime-identifier> -c Debug
   ```

2. Run the tests:  
   ```
   device-runners ios test --app <path/to/app.app> --results-directory <path/to/output>
   ```

3. View test results in the output directory:  
   ```
   <path/to/output>/TestResults.xml
   ```

## Complete Example

To build and test the app at the path `sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj` and get the test output at the path `artifacts` on Apple Silicon macOS:

```bash
# Build the test app
dotnet build sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
  -f net9.0-ios \
  -r iossimulator-arm64 \
  -c Debug

# Run tests
device-runners ios test \
  --app sample/test/DeviceTestingKitApp.DeviceTests/bin/Debug/net9.0-ios/iossimulator-arm64/DeviceTestingKitApp.DeviceTests.app \
  --results-directory artifacts/test-results

# Test result file will be: artifacts/test-results/TestResults.xml
```

## Command Options

### `ios test`

| Option | Default | Description |
|--------|---------|-------------|
| `--app` | *(required)* | Path to the .app application bundle |
| `--device` | *(booted simulator)* | iOS Simulator device ID |
| `--results-directory` | `artifacts` | Directory for test output files |
| `--port` | `16384` | TCP port to listen on for test results |
| `--connection-timeout` | `120` | Seconds to wait for initial connection |
| `--data-timeout` | `30` | Seconds to wait between data transmissions |

### `ios install`

| Option | Description |
|--------|-------------|
| `--app` | Path to the .app application bundle |
| `--device` | iOS Simulator device ID (optional) |

### `ios launch`

| Option | Description |
|--------|-------------|
| `--app` | Path to .app bundle (to determine bundle identifier) |
| `--bundle-id` | Bundle identifier to launch (alternative to `--app`) |
| `--device` | iOS Simulator device ID (optional) |

### `ios uninstall`

| Option | Description |
|--------|-------------|
| `--app` | Path to .app bundle (to determine bundle identifier) |
| `--bundle-id` | Bundle identifier to uninstall (alternative to `--app`) |
| `--device` | iOS Simulator device ID (optional) |
