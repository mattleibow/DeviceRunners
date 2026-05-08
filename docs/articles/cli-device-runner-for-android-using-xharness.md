# Prerequisites

Before running any tests on the CLI, you will need the XHarness .NET tool. For more information as to what that is and what it does, see the [Using XHarness](using-xharness.md) wiki.

# Running Tests

1. Build the app package for testing:  
   ```
   dotnet publish <path/to/app.csproj> -r <runtime-identifier> -f net7.0-android -c Release
   ```
2. Run the tests:  
   ```
   xharness android test --app <path/to/app.apk> --package-name <package-name> --instrumentation devicerunners.xharness.maui.XHarnessInstrumentation --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/TestResults.xml
   ```

To build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts` on my ARM64 Apple Silicon laptop:

```
dotnet publish sample/SampleMauiApp/SampleMauiApp.csproj \
  -r android-arm64 \
  -f net7.0-android \
  -c Release

xharness android test \
  --app sample/SampleMauiApp/bin/Release/net7.0-android/android-arm64/publish/com.companyname.samplemauiapp-Signed.apk \
  --package-name com.companyname.samplemauiapp \
  --instrumentation devicerunners.xharness.maui.XHarnessInstrumentation \
  --output-directory artifacts

# test result file will be artifacts/TestResults.xml
```

> **NOTES**
> * If you want to build a debug app and test that, you will also need to set `EmbedAssembliesIntoApk` to `True`:  
>   ```
>   dotnet publish ... -p:EmbedAssembliesIntoApk=true
>   ```

