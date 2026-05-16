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

## Testing with `dotnet test` (Recommended)

The simplest way to run device tests. Add the `DeviceRunners.Testing.Targets` NuGet package to your test project, then run:

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-maccatalyst
dotnet test MyApp.DeviceTests.csproj -f net10.0-ios
dotnet test MyApp.DeviceTests.csproj -f net10.0-android
dotnet test MyApp.DeviceTests.csproj -f net10.0-windows10.0.19041.0
```

This builds, deploys, and runs your test app, then collects TRX results automatically. No extra tooling needed.

More information: [Using dotnet test](https://mattleibow.github.io/DeviceRunners/articles/using-dotnet-test.html)

## Testing with the Visual Runner

Testing using the visual runner is just a matter of running the test app like any other app. This can be done via the CLI or in the IDE. Perfect for debugging individual tests.

| | | |
|:-:|:-:|:-:|
|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/386c00fa-05f3-476c-ae08-2594bf06c211)|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/6044737c-aaa7-4272-b2e0-07d8e1a31d9d)|![image](https://github.com/mattleibow/DeviceRunners/assets/1096616/c23bd064-e8d5-4a81-832e-9306219a32e9)|

More information: [Visual Runner in the IDE](https://mattleibow.github.io/DeviceRunners/articles/visual-runner-in-the-ide.html)

## Testing with the DeviceRunners CLI (Advanced)

For scenarios requiring fine-grained control over deployment, the DeviceRunners CLI tool can be used directly. This is what `dotnet test` uses under the hood.

More information: [Using DeviceRunners CLI](https://mattleibow.github.io/DeviceRunners/articles/using-devicerunners-cli.html)

## Testing with XHarness (Legacy)

For legacy CI setups, XHarness is still supported for Android, iOS, and Mac Catalyst.

More information: [Using XHarness](https://mattleibow.github.io/DeviceRunners/articles/using-xharness.html)

## Credits

This is repository contains revised code from a few places:

 - https://github.com/xunit/devices.xunit  
   This code was migrated to use .NET MAUI and then split into components so that the UI can be separate from the test runner.
 - https://github.com/xunit/uitest.xunit  
   This code was migrated to use .NET MAUI.
 - https://github.com/nunit/nunit.xamarin  
   This code was migrated to use .NET MAUI and then features were added to support running individual tests.
 - https://github.com/dotnet/maui  
   This was the home for a short while during the migration.
