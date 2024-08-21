# DeviceTestingKitApp Sample App

This folder contains a convoluted sample app with all the different ways to run and test it.

In a typical scenario, you would not actually do all of this, but rather pick a few
mechanisms depending on the app type and tests types.

## Projects

### The `src` Folder

This folder represents a ".NET MAUI app" in a somewhat convoluted format to demonstrate
the different mechanisms available for testing.

This folder has 3 sub-folders with different types of projects:

* `DeviceTestingKitApp`  
  This is the main app that is published to the app store. It is very simple and just
  contains a single page with some text, and image and a counter button.
* `DeviceTestingKitApp.MauiLibrary`  
  This is a .NET MAUI class library that use a few features of .NET MAUI in a class
  library (instead of an app) so that a normal unit test can run against these.
* `DeviceTestingKitApp.Library`  
  This is a normal class library that has a few view models and services.

### The `test` Folder

This folder represents the possible tests you may have for your app:

* `DeviceTestingKitApp.DeviceTests`  
  This is a device tests app to run all the tests on an actual device instead of just
  in the IDE on the host machine. The host machine will not be the most accurate test
  of the features in the app because many code paths may require or expect to be
  running in an app environment.
  This project also contains several sample tests to demosnstrate how to write tests that
  are designed to run on a device and expect device features.
* `DeviceTestingKitApp.MauiLibrary.XunitTests`  
  This is a set of unit tests (that can run on both the host machine and the device) that
  test the functionality of the .NET MAUI app components. In this example, it is testing
  the various XAML views and value converters that only exist in the context of .NET MAUI.
* `DeviceTestingKitApp.Library.NUnitTests`  
  This is a set of unit tests that test the functionality of view models and other
  framework-agnostic code.
