# Xunit.Runner.Devices

A device runner for the xUnit.net unit testing framework.

This is part a port of xunit/devices.xunit and part a collection of the work done in the dotnet/maui repository.

Not sure where this is going, but hopefully it becomes useful.

https://learn.microsoft.com/en-us/dotnet/maui/deployment/

## Testing with the Visual Runner

## Testing with XHarness

XHarness is primarily a command line tool that enables running xUnit like tests on Android, Apple iOS / tvOS / WatchOS / Mac Catalyst, WASI and desktop browsers (WASM). See https://github.com/dotnet/xharness

In order to test with xharness, you will have to install the CLI tool first:

Bash:
```bash
dotnet tool install Microsoft.DotNet.XHarness.CLI                                                   \
    --global                                                                                        \
    --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json \
    --version "8.0.0-prerelease*"
```

PowerShell:

```ps1
dotnet tool install Microsoft.DotNet.XHarness.CLI                                                   `
    --global                                                                                        `
    --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json `
    --version "8.0.0-ci*"
```

> The snippets above are used to install the xharness tool globally, however you can also remove the `--global` arguemnt to get the tool to install locally in the current working directory. If this is the case, then you will also need to prefix the xharness commands with `dotnet`. For example, if the sample commands below say `xharness apple test` you will need to do `dotnet xharness apple test`.

### iOS

1. Build the app package for testing: _(The `iossimulator-arm64` RID is for a simulator on my Apple Silicon laptop)_  
   ```
   dotnet build -f net7.0-ios -r iossimulator-arm64 -c Debug <path/to/app.csproj>
   ```
2. Run the tests:  
   ```
   xharness apple test --target ios-simulator-64 --app <path/to/app.app> --output-directory <path/to/output>
   ```
3. View test results in the output path: 
   ```
   <path/to/output>/xunit-test-ios-simulator-64-<YYYYMMDD>_<HHMMSS>.xml
   ```

So to build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts`:

```
dotnet build \
  -f net7.0-ios \
  -r iossimulator-arm64 \
  -c Debug \
  sample/SampleMauiApp/SampleMauiApp.csproj

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
> * If you want to reset the simulator before running a test and exit afterwards, add `--reset-simulator` to the test command.  
>   ```
>   xharness apple test ... --reset-simulator
>   ```


### Android


dotnet publish -r android-arm64 -f net7.0-android sample/SampleMauiApp -p:EmbedAssembliesIntoApk=true

dotnet publish -r android-arm64 -f net7.0-android -c Release sample/SampleMauiApp

xharness android test --app sample/SampleMauiApp/bin/Debug/net7.0-android/android-arm64/publish/com.companyname.samplemauiapp-Signed.apk --output-directory artifacts -v --package-name com.companyname.samplemauiapp --instrumentation xunit.runner.devices.xharness.maui.XHarnessInstrumentation


### Mac Catalyst

xharness apple test --app sample/SampleMauiApp/bin/Debug/net7.0-maccatalyst/maccatalyst-arm64/SampleMauiApp.app --output-directory artifacts --target maccatalyst -v
