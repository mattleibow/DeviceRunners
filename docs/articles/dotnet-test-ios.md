# iOS — `dotnet test`

> [!TIP]
> This guide covers iOS-specific details. See [Using dotnet test](using-dotnet-test.md) for general setup.

## Prerequisites

- macOS with Xcode installed
- An iOS simulator booted (or specify one via `DeviceRunnersDevice`)

### Simulator Setup

```bash
# Create and boot a simulator
dotnet apple simulator create "TestSimulator" --device-type "iPhone 16" --format json
dotnet apple simulator boot "TestSimulator" --wait
```

Or boot an existing simulator from Xcode's Devices & Simulators window.

## Running Tests

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-ios
```

The CLI automatically detects the booted simulator. To target a specific simulator:

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-ios \
  -p:DeviceRunnersDevice=ABCD1234-5678-90EF
```

## How It Works

### Environment Variables

On iOS, environment variables are passed to the app via the `SimCtl.LaunchAppAsync` API (AppleDev 0.8.10+), which uses the `SIMCTL_CHILD_*` convention internally. The CLI sets:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DEVICE_RUNNERS_AUTORUN` | `1` | Tells the app to auto-start tests |
| `DEVICE_RUNNERS_PORT` | `16384` (default) | TCP port to connect to on the host |
| `DEVICE_RUNNERS_HOST_NAMES` | `localhost` | Host address (simulator runs on the same machine) |

### Device Logs

The CLI captures a process-filtered device log after the test run using the unified logging system:

```
predicate: 'process == "MyApp.DeviceTests"'
```

This produces a concise log (~200 lines) containing:
- All `Console.WriteLine` output from the app
- Test pass/fail/skip messages
- **Full managed exception stack traces** if the app crashes

The log is saved to `<results-directory>/ios-device-log.txt`.

## Troubleshooting

### "No booted iOS simulator found"

Ensure a simulator is booted before running `dotnet test`:

```bash
dotnet apple simulator boot "iPhone 16" --wait
```

Or specify a simulator UDID:

```bash
dotnet test MyApp.csproj -f net10.0-ios \
  -p:DeviceRunnersDevice=$(xcrun simctl list devices booted -j | jq -r '.devices[][] | select(.isAvailable) | .udid' | head -1)
```

### App crashes with partial results

If you see `(incomplete: app crashed)` in the output, check the device log at `<results-directory>/ios-device-log.txt` for the full managed exception stack trace. Common causes:
