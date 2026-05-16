# Using `dotnet test` for Device Testing

> [!TIP]
> `dotnet test` integration is the **recommended** way to run device tests. It uses the standard .NET testing workflow, produces TRX results, and works in CI/CD without additional tooling.

## Overview

The `DeviceRunners.Testing.Targets` NuGet package hooks into `dotnet test` to automatically build, deploy, and run your device test app on the target platform, then collect results via TCP.

```bash
# Run on each platform
dotnet test MyApp.DeviceTests.csproj -f net10.0-android
dotnet test MyApp.DeviceTests.csproj -f net10.0-ios
dotnet test MyApp.DeviceTests.csproj -f net10.0-maccatalyst
dotnet test MyApp.DeviceTests.csproj -f net10.0-windows10.0.19041.0
dotnet test MyApp.BrowserTests.csproj  # Blazor WASM (no -f needed)
```

## Setup

### 1. Add the NuGet Package

Add `DeviceRunners.Testing.Targets` to your device test project:

```xml
<PackageReference Include="DeviceRunners.Testing.Targets" />
```

### 2. Configure Your App

In your `MauiProgram.cs`, call `AddCliConfiguration()` on the visual runner configuration:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseVisualTestRunner(conf => conf
            .AddCliConfiguration()     // Reads config from DeviceRunners CLI or dotnet test
            .AddConsoleResultChannel()
            .AddTestAssembly(typeof(MauiProgram).Assembly)
            .AddXunit())
        .ConfigureUITesting();

    return builder.Build();
}
```

When `dotnet test` runs your app, configuration is injected via environment variables (or CLI arguments for MSIX-packaged Windows apps where env vars cannot be forwarded). `AddCliConfiguration()` detects this and:
- Enables auto-start (runs all tests immediately)
- Connects back to the CLI tool via TCP to stream results
- Auto-terminates when complete

When neither environment variables nor CLI arguments are present (e.g., running from the IDE), the call is a no-op and the visual runner behaves normally.

### 3. Run Tests

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-maccatalyst
```

That's it. The package handles building the app, deploying it, launching it with the right environment variables, collecting results, and producing a TRX file.

## Output

The output matches the standard `dotnet test` format:

```
Results File: /path/to/test-results.trx
Test summary: total: 44, failed: 5, succeeded: 36, skipped: 3, duration: 14.8s
```

When tests fail:

```
error TESTERROR: Test summary: total: 44, failed: 5, succeeded: 36, skipped: 3, duration: 14.8s
```

### Crash Detection

If the app crashes during the test run (e.g., an unhandled exception on the UI thread), the CLI detects the missing "end" event and reports:

```
The application appears to have crashed during the test run.
Only 8 test result(s) were received before the connection was lost.
Check the device log for crash details.
```

```
error TESTERROR: Test summary: total: 8, failed: 1, succeeded: 6, skipped: 1, duration: 7.0s (incomplete: app crashed)
```

**Exit codes:**
| Code | Meaning |
|------|---------|
| 0 | All tests passed |
| 1 | One or more tests failed |
| 2 | App crashed (partial results) |

## Configuration

All settings are MSBuild properties that can be set in your `.csproj` or passed via `-p:`:

| Property | Default | Description |
|----------|---------|-------------|
| `DeviceRunnersPort` | `16384` | TCP port for test result collection |
| `DeviceRunnersConnectionTimeout` | `120` | Seconds to wait for the app to connect |
| `DeviceRunnersDataTimeout` | `30` | Seconds of silence before assuming the run ended |
| `DeviceRunnersDevice` | _(auto)_ | Target device ID (Android emulator serial, iOS simulator UDID) |
| `DeviceRunnersBin` | _(bundled)_ | Override the CLI tool path (for using a globally installed tool) |
| `DeviceRunnersWasmTimeout` | `300` | WASM: test execution timeout in seconds |

Example:

```bash
# Use a specific iOS simulator and longer timeout
dotnet test MyApp.csproj -f net10.0-ios \
  -p:DeviceRunnersDevice=ABCD1234-5678-90EF \
  -p:DeviceRunnersConnectionTimeout=180
```

## How It Works

The package replaces the standard `VSTest` MSBuild target for device platforms (Android, iOS, macOS Catalyst, Windows, Browser (WASM)). When you run `dotnet test`, the following happens:

1. **Build** — The app is compiled for the target platform (APK, .app bundle, .exe, or loose MSIX layout)
2. **Deploy** — The CLI tool installs the app on the device/simulator
3. **Launch** — The app is started with configuration (env vars or CLI args) that tells it to auto-run tests and connect back via TCP
4. **Collect** — The CLI listens on a TCP port for NDJSON test events and writes a TRX file
5. **Report** — Results are parsed and reported in the standard `dotnet test` format

### Configuration Injection

Each platform uses a different mechanism to pass configuration to the app:

| Platform | Mechanism |
|----------|-----------|
| Android | Baked into the APK at build time via `_GenerateEnvironmentFiles` |
| iOS | Passed via `SimCtl.LaunchAppAsync` (AppleDev 0.8.10+) |
| macOS Catalyst | `ProcessStartInfo.EnvironmentVariables` (direct process launch) |
| Windows (unpackaged) | `ProcessStartInfo.EnvironmentVariables` (direct process launch) |
| Windows (MSIX loose) | CLI arguments via `winapp.exe --args` (env vars cannot be forwarded to packaged apps) |
| Browser (WASM) | URL query string (`?device-runners-autorun=1`) | Set by CLI when launching Chrome |

> [!NOTE]
> On all platforms except Android, the CLI injects configuration at **launch time** — the same built app can be run with different settings without rebuilding. On Android, configuration is embedded into the APK at **build time** because `adb` has no mechanism to pass environment variables when launching an app. This means changing configuration (e.g., the TCP port) requires a rebuild. Launch-time injection via intent extras is tracked in [#123](https://github.com/mattleibow/DeviceRunners/issues/123).

### MSBuild Target Chain

```
VSTest
  -> Build                     (compile the app)
  -> _DeviceRunnersRunTests
       -> _DeviceRunnersPrepareArgs   (build CLI arguments)
       -> _DeviceRunnersExecTests     (run the CLI tool)
       -> _DeviceRunnersReportResults (parse TRX and report)
```

## Platform Guides

For platform-specific setup and prerequisites:

- [Android](dotnet-test-android.md) — Emulator setup, APK configuration
- [iOS](dotnet-test-ios.md) — Simulator setup, device logs
- [macOS Catalyst](dotnet-test-macos.md) — No simulator needed
- [Windows](dotnet-test-windows.md) — Unpackaged EXE and MSIX loose deploy
- [Browser (WASM)](dotnet-test-wasm.md) — Blazor WebAssembly browser testing

## Comparison with Other Approaches

| Feature | `dotnet test` | DeviceRunners CLI | XHarness |
|---------|:---:|:---:|:---:|
| Standard .NET workflow | Yes | No | No |
| TRX output | Yes | Yes | Yes |
| Crash detection | Yes | Yes | No |
| No extra tooling | Yes | Global tool | Global tool |
| CI-friendly | Yes | Yes | Yes |
| Interactive debugging | No (use Visual Runner) | No | No |
| MSIX packaged apps | Yes (loose deploy) | Yes | Yes |

> [!NOTE]
> For MSIX-packaged Windows apps, `dotnet test` uses loose-file MSIX registration (the app is registered directly from the build output folder via `winapp.exe`). No `dotnet publish`, certificate signing, or MSIX packaging step is needed. For scenarios requiring fine-grained control over deployment, use the [DeviceRunners CLI](using-devicerunners-cli.md) directly.
