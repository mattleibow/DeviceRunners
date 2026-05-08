# Mac Catalyst Testing with XHarness

## Prerequisites

Before running any tests on the CLI, you will need the XHarness .NET tool. For more information as to what that is and what it does, see the [Using XHarness](using-xharness.md) article.

## Running Tests

1. Build the app package for testing:
   ```
   dotnet build <path/to/app.csproj> -f net9.0-maccatalyst -r <runtime-identifier> -c Debug
   ```
2. Run the tests:  
   ```
   xharness apple test --target maccatalyst --app <path/to/app.app> --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/xunit-test-maccatalyst-<YYYYMMDD>_<HHMMSS>.xml
   ```

To build and test the app at the path `sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj` and get the test output at the path `artifacts` on my ARM64 Apple Silicon laptop:

```
dotnet build sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
  -f net9.0-maccatalyst \
  -r maccatalyst-arm64 \
  -c Debug

xharness apple test \
  --target maccatalyst \
  --app sample/test/DeviceTestingKitApp.DeviceTests/bin/Debug/net9.0-maccatalyst/maccatalyst-arm64/DeviceTestingKitApp.DeviceTests.app \
  --output-directory artifacts

# test result file will be artifacts/xunit-test-maccatalyst-########_######.xml
```
