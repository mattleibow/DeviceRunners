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
| `.msix` (packaged) | _(default/MSIX)_ | CLI installs and launches the MSIX package |

You don't need to set `WindowsPackageType` — the targets find whichever artifact exists.

### Environment Variables

For unpackaged apps, environment variables are injected directly into the process at launch time:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DEVICE_RUNNERS_AUTORUN` | `1` | Tells the app to auto-start tests |
| `DEVICE_RUNNERS_PORT` | `16384` (default) | TCP port to connect to on the host |
| `DEVICE_RUNNERS_HOST_NAMES` | `localhost` | Host address |

For MSIX-packaged apps, the CLI handles installation, certificate management, launching, and cleanup.

### Unpackaged Mode

If your project sets `WindowsPackageType=None`, the build produces a plain `.exe`. This is the simplest workflow — no certificate management, no package installation, no cleanup.

### Packaged (MSIX) Mode

If your project uses the default Windows packaging (or explicitly sets `WindowsPackageType=MSIX`), `dotnet test` will find the `.msix` in the output and use the CLI's full MSIX workflow (install certificate, install package, launch, collect results, uninstall).

## Troubleshooting

### "No .exe or .msix found"

Ensure the project targets a Windows TFM and builds successfully. Check the build output directory for the expected artifact.

### Firewall Prompts

The first time you run tests, Windows may prompt to allow the app through the firewall (for the TCP connection on port 16384). Allow it for the test to proceed.
