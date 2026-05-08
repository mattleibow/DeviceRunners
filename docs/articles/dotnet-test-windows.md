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

### Unpackaged Mode

The Testing.Targets package defaults to `WindowsPackageType=None`, which produces a plain `.exe` instead of an MSIX package. This simplifies the workflow — no certificate management, no package installation, no cleanup.

The CLI locates the `.exe` in the build output and launches it directly with environment variables:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DEVICE_RUNNERS_AUTORUN` | `1` | Tells the app to auto-start tests |
| `DEVICE_RUNNERS_PORT` | `16384` (default) | TCP port to connect to on the host |
| `DEVICE_RUNNERS_HOST_NAMES` | `localhost` | Host address |

### MSIX Packaged Apps

If you need MSIX packaging (e.g., for testing APIs that require package identity), override the default:

```bash
dotnet test MyApp.csproj -f net10.0-windows10.0.19041.0 \
  -p:WindowsPackageType=MSIX
```

> [!NOTE]
> MSIX packaging with `dotnet test` is not fully supported yet. For MSIX-packaged Windows apps, use the [DeviceRunners CLI](cli-device-runner-for-windows-using-devicerunners-cli.md) which handles certificate management and package installation.

## Troubleshooting

### "Executable not found"

Ensure the project targets a Windows TFM and that `WindowsPackageType` is `None` (the default when using Testing.Targets). If you've set `WindowsPackageType=MSIX` elsewhere, the build produces an MSIX instead of an EXE.

### Firewall Prompts

The first time you run tests, Windows may prompt to allow the app through the firewall (for the TCP connection on port 16384). Allow it for the test to proceed.
