# Test Device Runners

A set of device runners for various testing frameworks.

The current platforms are:

 - Android
 - iOS
 - macOS (using Mac Catalyst)
 - Windows (using WinUI 3)

The current testing frameworks supported are:

 - Xunit
    - Visual device runner
    - XHarness (CI) device runner
 - NUnit
    - Visual device runner

## Testing with the Visual Runner

Testing using the visual runner is just a matter of running the test app like any other app. This can be done via the CLI or in the IDE.

| | | |
|:-:|:-:|:-:|
|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/386c00fa-05f3-476c-ae08-2594bf06c211)|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/6044737c-aaa7-4272-b2e0-07d8e1a31d9d)|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/c23bd064-e8d5-4a81-832e-9306219a32e9)|


## Testing with XHarness

XHarness is primarily a command line tool that enables running tests on Android, iOS, tvOS, Mac Catalyst, WASI and desktop browsers (WASM). See https://github.com/dotnet/xharness

> XHarness is not available on nuget.org at this time, so an additional feed is required:  
> https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json

In order to test with xharness, you will have to install the CLI tool first:

```
dotnet tool install Microsoft.DotNet.XHarness.CLI \
  --global \
  --version "8.0.0-prerelease*" \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json
```

> The snippets above are used to install the xharness tool globally, however you can also remove the `--global` arguemnt to get the tool to install locally in the current working directory. If this is the case, then you will also need to prefix the xharness commands with `dotnet`. For example, if the sample commands below say `xharness apple test` you will need to do `dotnet xharness apple test`.

### iOS

1. Build the app package for testing:
   ```
   dotnet build <path/to/app.csproj> -f net7.0-ios -r <runtime-identifier> -c Debug
   ```
2. Run the tests:  
   ```
   xharness apple test --target ios-simulator-64 --app <path/to/app.app> --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/xunit-test-ios-simulator-64-<YYYYMMDD>_<HHMMSS>.xml
   ```

To build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts` on my ARM64 Apple Silicon laptop:

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

#### Managing Android Emulators

Because XHarness does not yet boot or create Android emulators, we will need to make use of another tool: `AndroidSDK.Tool` - a global dotnet tool for various android adb, avd, and emulator needs. See https://github.com/redth/AndroidSdk.Tools

```
dotnet tool install --global AndroidSDK.Tool
```

Once the tool is installed, you can create and boot an emulator:

1. Install the emulator image using the Android SDK:  
   ```
   android sdk install --package "system-images;android-<android-api-level>;google_apis;<cpu-architecture>"
   ```
2. Create the emulator instance:  
   ```
   android avd create --name <emulator-name> --sdk "system-images;android-<android-api-level>;google_apis;<cpu-architecture>" --device <device-type>
   ```
3. Boot the emulator:  
   ```
   android avd start --name <emulator-name> --wait-boot
   ```
4. Run tests using Xharness. _(See steps above)_
5. Shutdown the emulator using XHarness:  
   ```
   xharness android adb -- emu kill
   ```

To download, install, create and boot a Pixel 5 emulator running Android 14 (API Level 34) on my ARM64 Apple Silicon laptop:

```
android sdk install \
  --package "system-images;android-34;google_apis;arm64-v8a"

android avd create \
  --name TestRunnerEmulator \
  --sdk "system-images;android-34;google_apis;arm64-v8a" \
  --device pixel_5

android avd start \
  --name TestRunnerEmulator \
  --wait-boot

# run things on the emulator

xharness android adb -- emu kill
```

> **NOTES**
> * If you want to build a debug app and test that, you will also need to set `EmbedAssembliesIntoApk` to `True`:  
>   ```
>   dotnet publish ... -p:EmbedAssembliesIntoApk=true
>   ```


### Mac Catalyst

1. Build the app package for testing:
   ```
   dotnet build <path/to/app.csproj> -f net7.0-maccatalyst -r <runtime-identifier> -c Debug
   ```
2. Run the tests:  
   ```
   xharness apple test --target maccatalyst --app <path/to/app.app> --output-directory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/xunit-test-maccatalyst-<YYYYMMDD>_<HHMMSS>.xml
   ```

To build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts` on my ARM64 Apple Silicon laptop:

```
dotnet build sample/SampleMauiApp/SampleMauiApp.csproj \
  -f net7.0-maccatalyst \
  -r maccatalyst-arm64 \
  -c Debug

xharness apple test \
  --target maccatalyst \
  --app sample/SampleMauiApp/bin/Debug/net7.0-maccatalyst/maccatalyst-arm64/SampleMauiApp.app \
  --output-directory artifacts

# test result file will be artifacts/xunit-test-maccatalyst-########_######.xml
```


### Windows

> XHarness does not _yet_ support launching Windows tests, but this is easy to work around with a small powershell script. These scripts are found in the `./scripts/` folder.

1. Build the app package for testing:
   ```
   dotnet publish <path/to/app.csproj> -f net7.0-windows10.0.<version>.0 -c Release -p:AppxPackageSigningEnabled=true
   ```
2. Run the tests:  
   ```
   ./scripts/Start-Tests.ps1 -AppPackage <path/to/app.msix> -OutputDirectory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/TestResults.xml
   ```

To build and test the app at the path `sample/SampleMauiApp/SampleMauiApp.csproj` and get the test output at the path `artifacts` on my Windows laptop:

```
$fingerprint = .\scripts\New-Certificate.ps1 -Project sample\SampleMauiApp\SampleMauiApp.csproj
dotnet publish sample\SampleMauiApp\SampleMauiApp.csproj `
  -f net7.0-windows10.0.19041.0 `
  -c Release `
  -p:AppxPackageSigningEnabled=true `
  -p:PackageCertificateThumbprint=$fingerprint `
  -p:PackageCertificateKeyFile=""

./scripts/Start-Tests.ps1 `
  -App sample\SampleMauiApp\bin\Release\net7.0-windows10.0.19041.0\win10-x64\AppPackages\SampleMauiApp_1.0.0.1_Test\SampleMauiApp_1.0.0.1_x64.msix `
  -OutputDirectory artifacts

# test result file will be artifacts/TestResults.xml
```

> If the app certificate is not installed, then an admin prompt will popup asking for permissions to install the certificate. If the test run is already elevated, then it will silently install (and uninstall).

## UI Testing Support

TODO

## Credits

This is repository contains revised code from a few places:

 - a port of https://github.com/xunit/devices.xunit to use .NET MAUI
 - a port of https://github.com/xunit/uitest.xunit to use .NET MAUI
 - a port of https://github.com/nunit/nunit.xamarin to use .NET MAUI
 - parts of the work done in https://github.com/dotnet/maui
