# macOS Testing with DeviceRunners CLI


This guide covers testing Mac Catalyst applications using the DeviceRunners CLI tool.

## Running Tests

The DeviceRunners CLI tool provides a comprehensive test workflow for Mac Catalyst applications that handles installation, execution, and cleanup automatically.

1. Build the app bundle for testing:  
   ```
   dotnet publish <path/to/app.csproj> -f net9.0-maccatalyst -r <runtime-identifier> -c Release
   ```

2. Run the tests:  
   ```
   device-runners macos test --app <path/to/app.app> --results-directory <path/to/output>
   ```

3. View test results in the output directory:  
   ```
   <path/to/output>/TestResults.xml
   ```

## Complete Example

To build and test the app at the path `sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj` and get the test output at the path `artifacts` on Apple Silicon macOS:

```bash
# Build the test app
dotnet publish sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
  -f net9.0-maccatalyst \
  -r maccatalyst-arm64 \
  -c Release

# Run tests
device-runners macos test \
  --app sample/test/DeviceTestingKitApp.DeviceTests/bin/Release/net9.0-maccatalyst/maccatalyst-arm64/publish/DeviceTestingKitApp.DeviceTests.app \
  --results-directory artifacts/test-results

# Test result file will be: artifacts/test-results/TestResults.xml
```
