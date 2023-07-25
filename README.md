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

More information can be found in the wiki: [Visual Runner in the IDE](https://github.com/mattleibow/DeviceRunners/wiki/Visual-Runner-in-the-IDE)

## Testing with the CLI

Test can also be run on the CLI - both locally and on CI. For tests on Android, iOS and Mac Catalyst, there is the XHarness tool. For Windows, all we need is PowerShell.

More information can be found in the wiki: 

* [Using XHarness](https://github.com/mattleibow/DeviceRunners/wiki/Using-XHarness)
* [iOS - XHarness](https://github.com/mattleibow/DeviceRunners/wiki/CLI-Device-Runner-for-iOS-using-XHarness)   
* [Android - XHarness](https://github.com/mattleibow/DeviceRunners/wiki/CLI-Device-Runner-for-Android-using-XHarness)  
* [Mac Catalyst - XhHarness](https://github.com/mattleibow/DeviceRunners/wiki/CLI-Device-Runner-for-Mac-Catalyst-using-XHarness)  
* [Windows - PowerShell](https://github.com/mattleibow/DeviceRunners/wiki/CLI-Device-Runner-for-Windows-using-PowerShell)  


## UI Testing Support

More information can be found in the wiki: [UI Tests](https://github.com/mattleibow/DeviceRunners/wiki/UI-Tests)

## Credits

This is repository contains revised code from a few places:

 - https://github.com/xunit/devices.xunit  
   This code was migrated to use .NET MAUI and then split into components so that the UI can be separate from the test runner.
 - a port of https://github.com/xunit/uitest.xunit to use .NET MAUI
 - a port of https://github.com/nunit/nunit.xamarin to use .NET MAUI
 - parts of the work done in https://github.com/dotnet/maui
