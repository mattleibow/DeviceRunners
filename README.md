# Xunit.Runner.Devices

A device runner for the xUnit.net unit testing framework.

This is part a port of xunit/devices.xunit and part a collection of the work done in the dotnet/maui repository.

Not sure where this is going, but hopefully it becomes useful.

https://learn.microsoft.com/en-us/dotnet/maui/deployment/

## Testing with the Visual Runner

## Testing with XHarness

XHarness is primarily a command line tool that enables running xUnit like tests on Android, Apple iOS / tvOS / WatchOS / Mac Catalyst, WASI and desktop browsers (WASM). See https://github.com/dotnet/xharness

In order to test with xharness, you will have to install the CLI tool first:

```
dotnet tool install Microsoft.DotNet.XHarness.CLI \
  --global \
  --version "8.0.0-prerelease*" \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json
```

> The snippets above are used to install the xharness tool globally, however you can also remove the `--global` arguemnt to get the tool to install locally in the current working directory. If this is the case, then you will also need to prefix the xharness commands with `dotnet`. For example, if the sample commands below say `xharness apple test` you will need to do `dotnet xharness apple test`.

### iOS

1. Build the app package for testing: _(The `iossimulator-arm64` RID is for a simulator on my Apple Silicon laptop)_  
   ```
   dotnet build <path/to/app.csproj> -f net7.0-ios -r iossimulator-arm64 -c Debug
   ```
2. Run the tests:  
   ```
   xharness apple test --target ios-simulator-64 --app <path/to/app.app> --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/xunit-test-ios-simulator-64-<YYYYMMDD>_<HHMMSS>.xml
   ```

To build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts`:

```
dotnet build sample/SampleMauiApp/SampleMauiApp.csproj \
  -f net7.0-ios \
  -r iossimulator-arm64 \
  -c Debug

xharness apple test \
  --target ios-simulator-64 \
  --app sample/SampleMauiApp/bin/Debug/net7.0-ios/iossimulator-arm64/SampleMauiApp.app \
  --output-directory artifacts

# test result file will be artifacts/xunit-test-ios-simulator-64-########_######.xml
```

> **NOTES**
> * It appears that you cannot publish for a simulator and I am having issues building a Release app for iOS.
> * If you want to laucn on a specific device, you can pass `--device <UDID>` using the UDID from running:  
>   ```
>   xharness apple state --include-simulator-uuid
>   ```
> * If you want to reset the simulator before running a test and exit afterwards, add `--reset-simulator` to the test command:  
>   ```
>   xharness apple test ... --reset-simulator
>   ```


### Android

1. Build the app package for testing:  
   ```
   dotnet publish <path/to/app.csproj> -r android-arm64 -f net7.0-android -c Release
   ```
2. Run the tests:  
   ```
   xharness android test --app <path/to/app.apk> --package-name <package-name> --instrumentation xunit.runner.devices.xharness.maui.XHarnessInstrumentation --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/TestResults.xml
   ```

To build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts`:

```
dotnet publish sample/SampleMauiApp/SampleMauiApp.csproj \
  -r android-arm64 \
  -f net7.0-android \
  -c Release

xharness android test \
  --app sample/SampleMauiApp/bin/Release/net7.0-android/android-arm64/publish/com.companyname.samplemauiapp-Signed.apk \
  --package-name com.companyname.samplemauiapp \
  --instrumentation xunit.runner.devices.xharness.maui.XHarnessInstrumentation \
  --output-directory artifacts

# test result file will be artifacts/TestResults.xml
```

Because XHarness does not yet boot or create Android emulators, we will need to make use of another tool: `AndroidSDK.Tool` - a global dotnet tool for various android adb, avd, and emulator needs. See https://github.com/redth/AndroidSdk.Tools

```
dotnet tool install --global AndroidSDK.Tool
```


> **NOTES**
> * If you want to build a debug app and test that, you will also need to set `EmbedAssembliesIntoApk` to `True`:  
>   ```
>   dotnet publish ... -p:EmbedAssembliesIntoApk=true
>   ```

### Mac Catalyst

xharness apple test --app sample/SampleMauiApp/bin/Debug/net7.0-maccatalyst/maccatalyst-arm64/SampleMauiApp.app --output-directory artifacts --target maccatalyst -v
