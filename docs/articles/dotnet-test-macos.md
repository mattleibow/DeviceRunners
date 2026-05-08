# macOS Catalyst — `dotnet test`

> [!TIP]
> This guide covers macOS Catalyst-specific details. See [Using dotnet test](using-dotnet-test.md) for general setup.

## Prerequisites

- macOS (no simulator or emulator needed — the app runs natively)

## Running Tests

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-maccatalyst
```

This is the simplest platform — no device setup required.

## How It Works

### Environment Variables

On macOS Catalyst, the CLI launches the `.app` bundle directly as a child process using `ProcessStartInfo`. Environment variables are injected into the process at launch time:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DEVICE_RUNNERS_AUTORUN` | `1` | Tells the app to auto-start tests |
| `DEVICE_RUNNERS_PORT` | `16384` (default) | TCP port to connect to on the host |
| `DEVICE_RUNNERS_HOST_NAMES` | `localhost` | Host address |

### App Bundle Discovery

The MSBuild targets locate the `.app` bundle in the build output directory using `Directory.GetDirectories('$(OutputPath)', '*.app')`. Standard MSBuild glob patterns (`*.app/`) do not reliably match directories on macOS, so the targets use the .NET `System.IO.Directory` API instead.

### Why Direct Launch Instead of `open`

The CLI launches the app directly (via `ProcessStartInfo`) rather than using the macOS `open` command. While `open --env KEY=VALUE` does support environment variables, direct launch is preferred because:
- The CLI retains the process handle and can monitor the app's lifecycle
- Exit codes are captured directly
- No dependency on Launch Services behavior

## Troubleshooting

### App crashes immediately

Check the build output to ensure the `.app` bundle was produced. If using `debug` configuration, the app should run without code signing issues on macOS Catalyst.

If the app crashes with a managed exception, check the console output — on macOS Catalyst, stdout/stderr from the app process is captured directly by the CLI.
