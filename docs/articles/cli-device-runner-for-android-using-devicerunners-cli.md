# Android Testing with DeviceRunners CLI

> [!NOTE]
> This documentation was partially generated using AI and may contain mistakes or be missing information. Please verify commands and procedures before use, and report any issues or improvements needed.

This guide covers testing Android applications using the DeviceRunners CLI tool.

## Running Tests

The DeviceRunners CLI tool provides a comprehensive test workflow that handles installation, execution, and cleanup automatically.

1. Build the app package for testing:  
   ```
   dotnet publish <path/to/app.csproj> -r <runtime-identifier> -f net9.0-android -c Release
   ```

2. Run the tests:  
   ```
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
  -f net9.0-android \
  -r android-x64 \
  -c Release

# Run tests
device-runners android test \
  --app sample/test/DeviceTestingKitApp.DeviceTests/bin/Release/net9.0-android/android-x64/publish/com.companyname.devicetestingkitapp.devicetests-Signed.apk \
  --results-directory artifacts/test-results

# Test result files will be:
# - artifacts/test-results/TestResults.xml
# - artifacts/test-results/logcat.txt
```
