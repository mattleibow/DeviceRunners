# Android Testing with DeviceRunners CLI

This guide covers testing Android applications using the DeviceRunners CLI tool.

## Prerequisites

- A running Android emulator or connected device (see [Managing Android Emulators](managing-android-emulators.md))
- .NET 9.0+ SDK with MAUI workloads installed

## Running Tests

The DeviceRunners CLI tool provides a comprehensive test workflow that handles installation, execution, and cleanup automatically.

1. Build the app package for testing:  
   ```bash
   dotnet publish <path/to/app.csproj> -r <runtime-identifier> -f net10.0-android -c Release
   ```
   Common runtime identifiers: `android-arm64` (physical devices, Apple Silicon emulators), `android-x64` (Intel emulators)

2. Run the tests:  
   ```bash
   device-runners android test --app <path/to/app.apk> --results-directory <path/to/output>
   ```

3. View test results:  
   ```
   <path/to/output>/TestResults.xml
   <path/to/output>/logcat.txt
   ```

## Complete Example

To build and test the app at the path `sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj`:

```bash
# Build the test app
dotnet publish sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
  -f net10.0-android \
  -r android-arm64 \
  -c Release

# Run tests
device-runners android test \
  --app sample/test/DeviceTestingKitApp.DeviceTests/bin/Release/net10.0-android/android-arm64/publish/com.companyname.devicetestingkitapp.devicetests-Signed.apk \
  --results-directory artifacts/test-results

# Test result files will be:
# - artifacts/test-results/TestResults.xml
# - artifacts/test-results/logcat.txt
```

## See Also

- **[Using DeviceRunners CLI](using-devicerunners-cli.md)** - CLI overview and all platform commands
- **[Managing Android Emulators](managing-android-emulators.md)** - Setting up emulators
- **[CLI Test Workflow](devicerunners-cli-test-workflow.md)** - Detailed workflow internals
