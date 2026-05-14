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

The MSBuild targets locate the `.app` bundle in the build output directory using `Directory.GetDirectories('$(OutputPath)', '$(AssemblyName).app')`. Standard MSBuild glob patterns (`*.app/`) do not reliably match directories on macOS, so the targets use the .NET `System.IO.Directory` API instead.

### Why Direct Launch Instead of `open`

The CLI launches the `.app` bundle directly (via `Process.Start`) rather than using the macOS `open` command. While `open --env KEY=VALUE` does support environment variables, direct launch is preferred because:
- Environment variables can be injected via `ProcessStartInfo`
- No dependency on Launch Services behavior

> [!NOTE]
> The CLI does not currently retain the process handle after launch — the app is expected to auto-terminate via `AddCliConfiguration()` after tests complete. If the app hangs, it must be terminated manually.

## Troubleshooting

### App crashes immediately

Check the build output to ensure the `.app` bundle was produced. If using `debug` configuration, the app should run without code signing issues on macOS Catalyst.
