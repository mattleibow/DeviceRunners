# iOS Testing with XHarness

## Prerequisites

Before running any tests on the CLI, you will need the XHarness .NET tool. For more information as to what that is and what it does, see the [Using XHarness](using-xharness.md) article.

## Running Tests

1. Build the app package for testing:
   ```
   dotnet build <path/to/app.csproj> -f net9.0-ios -r <runtime-identifier> -c Debug
   ```
2. Run the tests:  
   ```
   xharness apple test --target ios-simulator-64 --app <path/to/app.app> --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/xunit-test-ios-simulator-64-<YYYYMMDD>_<HHMMSS>.xml
   ```

To build and test the app at the path `sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj` and get the test output at the path `artifacts` on my ARM64 Apple Silicon laptop:

```
dotnet build sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj \
  -f net9.0-ios \
  -r iossimulator-arm64 \
  -c Debug

xharness apple test \
  --target ios-simulator-64 \
  --app sample/test/DeviceTestingKitApp.DeviceTests/bin/Debug/net9.0-ios/iossimulator-arm64/DeviceTestingKitApp.DeviceTests.app \
  --output-directory artifacts

# test result file will be artifacts/xunit-test-ios-simulator-64-########_######.xml
```

> [!NOTE]
> It appears that you cannot publish for a simulator — use `dotnet build` with Debug configuration instead.

> [!TIP]
> To launch on a specific device, pass `--device <UDID>` using the UDID from running:  
>   ```bash
>   xharness apple state --include-simulator-uuid
>   ```
> To reset the simulator before running a test and exit afterwards, add `--reset-simulator` to the test command.
