# Windows — `dotnet test`

> [!TIP]
> This guide covers Windows-specific details. See [Using dotnet test](using-dotnet-test.md) for general setup.

## Prerequisites

- Windows 10/11

## Running Tests

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-windows10.0.19041.0
```

## How It Works

### Automatic Detection

The MSBuild targets automatically detect what the build produced and adapt:

| Build Output | `WindowsPackageType` | How Tests Run |
|-------------|---------------------|---------------|
| `.exe` (unpackaged) | `None` | CLI launches the `.exe` directly with environment variables |
| `AppxManifest.xml` (loose MSIX layout) | _(default/MSIX)_ | CLI registers the app via `winapp.exe` and launches it with CLI arguments |

You don't need to set `WindowsPackageType` — the targets find whichever artifact exists.

### Environment Variables

For unpackaged apps, environment variables are injected directly into the process at launch time:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DEVICE_RUNNERS_AUTORUN` | `1` | Tells the app to auto-start tests |
| `DEVICE_RUNNERS_PORT` | `16384` (default) | TCP port to connect to on the host |
| `DEVICE_RUNNERS_HOST_NAMES` | `localhost` | Host address |

For loose MSIX apps, the same configuration is passed as CLI arguments (`--device-runners-autorun`, `--device-runners-port`, `--device-runners-host-names`) via `winapp.exe --args`, because environment variables cannot be forwarded to packaged app processes.

### Unpackaged Mode

If your project sets `WindowsPackageType=None`, the build produces a plain `.exe`. This is the simplest workflow — no certificate management, no package installation, no cleanup.

### MSIX Packaged Apps (Loose Deploy)

When the project uses the default Windows packaging (MSIX), the build output contains an `AppxManifest.xml`. The CLI detects this and uses loose-file MSIX registration via `winapp.exe` to register and launch the app directly from the build output — no `dotnet publish`, no certificate signing, no MSIX packaging needed.

This works automatically with `dotnet test`:

```bash
dotnet test MyApp.DeviceTests.csproj -f net10.0-windows10.0.19041.0
```

## Troubleshooting

### "No Windows app found"

Ensure the project targets a Windows TFM and builds successfully. The targets look for either a `.exe` (unpackaged) or an `AppxManifest.xml` (loose MSIX) in the build output.

### Firewall Prompts

The first time you run tests, Windows may prompt to allow the app through the firewall (for the TCP connection on port 16384). Allow it for the test to proceed.
