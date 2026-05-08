# Managing Android Emulators


This guide covers setting up and managing Android emulators for use with the DeviceRunners CLI tool.

## Overview

The DeviceRunners CLI tool works with any running Android emulator or connected device. For comprehensive emulator management, we recommend using the `AndroidSDK.Tool`, which provides a convenient .NET interface for Android SDK operations.

## Installing AndroidSdk.Tool

> [!TIP]
> Use `android …` if installed globally, or `dotnet android …` if restored as a local tool via `dotnet tool restore`.

```bash
dotnet tool install --global AndroidSdk.Tool
```

## Creating and Starting an Emulator

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
```
