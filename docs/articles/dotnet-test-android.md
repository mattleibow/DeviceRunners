# Android — `dotnet test`

> [!TIP]
> This guide covers Android-specific details. See [Using dotnet test](using-dotnet-test.md) for general setup.

## Prerequisites

- An Android emulator running (or a physical device connected via USB)
- .NET MAUI Android workload installed

### Emulator Setup

If you don't have an emulator running, see [Managing Android Emulators](managing-android-emulators.md) or use:

```bash
# Install SDK packages
dotnet android sdk install --package 'platform-tools' --package 'emulator' \
  --package 'system-images;android-36;google_apis;x86_64'

# Create and start an emulator
dotnet android avd create --name TestEmulator \
  --sdk 'system-images;android-36;google_apis;x86_64' --force
dotnet android avd start --name TestEmulator --wait --no-window \
  --gpu swiftshader_indirect --no-snapshot --no-audio --no-boot-anim
```

## Running Tests

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-android
```

To target a specific emulator or device:

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-android \
  -p:DeviceRunnersDevice=emulator-5554
```

## How It Works

### Environment Variables

On Android, environment variables cannot be set at launch time via `adb`. Instead, the Testing.Targets package injects them at **build time** using the `_GenerateEnvironmentFiles` MSBuild mechanism. The Mono runtime reads these from `__environment__.txt` inside the APK at startup.

This is unlike all other platforms where the CLI injects configuration at launch time, meaning the same built app can be reused with different settings. On Android, changing the TCP port or host names requires a rebuild.

> [!TIP]
> Launch-time injection via Android intent extras is planned for a future release. See [#123](https://github.com/mattleibow/DeviceRunners/issues/123).

The following variables are injected:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DEVICE_RUNNERS_AUTORUN` | `1` | Tells the app to auto-start tests |
| `DEVICE_RUNNERS_PORT` | `16384` (default) | TCP port to connect to on the host |
| `DEVICE_RUNNERS_HOST_NAMES` | `localhost;10.0.2.2` | Host addresses to try (`10.0.2.2` is the emulator gateway to the host) |

### APK Configuration

The package sets `EmbedAssembliesIntoApk=true` by default. This is required because the DeviceRunners CLI installs the APK via `adb install`, which doesn't handle .NET Android's "Fast Deployment" separate assembly push. Without this, the app crashes at startup with "No assemblies found."

> [!NOTE]
> This makes debug builds slightly slower (same as release builds) but ensures the APK is self-contained.

## Troubleshooting

### "Connection timed out"

The app uses `10.0.2.2` to reach the host machine from the Android emulator. If using a physical device, ensure the device can reach the host machine on the configured port, or use `adb reverse`:

```bash
adb reverse tcp:16384 tcp:16384
```

### "No assemblies found ... Fast Deployment"

Ensure `EmbedAssembliesIntoApk` is `true` (the Testing.Targets package sets this by default). If you've overridden it in your project, set it back:

```xml
<PropertyGroup>
  <EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
</PropertyGroup>
```
