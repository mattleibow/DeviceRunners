# Windows Testing with DeviceRunners CLI


This guide covers testing Windows applications using the DeviceRunners CLI tool. The tool supports three modes: packaged (.msix), loose-file MSIX (folder), and unpackaged (.exe) Windows applications.

## Running Tests

1. Build or publish the app for testing:  
   ```
   dotnet build <path/to/app.csproj> -f net10.0-windows10.0.19041.0 -c Release
   ```

2. Run the tests:  
   ```
   device-runners windows test --app <path/to/app.[msix|exe]|folder> --results-directory <path/to/output>
   ```

3. View test results:  
   ```
   <path/to/output>/TestResults.xml
   ```

## Input Modes

| `--app` value | Mode | Certificate? | Build step |
|---|---|---|---|
| `path/to/app.msix` | Packaged MSIX | Yes | `dotnet publish` |
| `path/to/build-output/` | Loose-file MSIX | No | `dotnet build` |
| `path/to/AppxManifest.xml` | Loose-file MSIX | No | `dotnet build` |
| `path/to/app.exe` | Unpackaged executable | No | `dotnet publish -p:WindowsPackageType=None` |

## Complete Examples

### Testing with Loose-File MSIX Layout (Recommended for Development)

The fastest way to test — just `dotnet build`, no publish or certificate needed. Requires **Windows Developer Mode** enabled on the machine.

```pwsh
# Build the app (no publish needed)
dotnet build sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj `
  -f net10.0-windows10.0.19041.0 `
  -c Release `
  -p:TestingMode=NonInteractiveVisual

# Run tests from the build output folder
device-runners windows test `
  --app artifacts/bin/DeviceTestingKitApp.DeviceTests/release_net10.0-windows10.0.19041.0 `
  --results-directory artifacts/test-results `
  --logger trx
```

The CLI detects the folder contains an `AppxManifest.xml` and uses the bundled `winapp.exe` to:
1. Register the app from loose files (no MSIX package needed)
2. Launch the app via COM activation
3. Collect test results via TCP
4. Terminate and unregister the development package

> **Note:** On CI, the Windows App Runtime framework must be installed from the NuGet cache before running. See [CI Pipeline Configuration](ci-pipeline.md) for details.

### Testing a Packaged Application

To build and test a packaged app at `sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj`:

```pwsh
# Create and install certificate automatically
$certResult = device-runners windows cert install `
  --project sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj `
  --output Json
$fingerprint = ($certResult | ConvertFrom-Json).Thumbprint

# Build the packaged app
dotnet publish sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj `
  -f net10.0-windows10.0.19041.0 `
  -c Release `
  -p:AppxPackageSigningEnabled=true `
  -p:PackageCertificateThumbprint=$fingerprint `
  -p:PackageCertificateKeyFile=""

# Clean up certificate after build
device-runners windows cert uninstall --fingerprint $fingerprint

# Run tests (the CLI tool will handle certificate installation automatically)
$msix = (Get-ChildItem -Recurse -Filter "*.msix" -Path "artifacts" | Select-Object -First 1).FullName
device-runners windows test --app $msix --results-directory artifacts/test-results --logger trx
```

### Testing an Unpackaged Application

```pwsh
# Build the unpackaged app  
dotnet publish sample/test/DeviceTestingKitApp.DeviceTests/DeviceTestingKitApp.DeviceTests.csproj `
  -f net10.0-windows10.0.19041.0 `
  -c Release `
  -p:WindowsPackageType=None `
  --output artifacts/publish

# Run tests
device-runners windows test `
  --app artifacts/publish/DeviceTestingKitApp.DeviceTests.exe `
  --results-directory artifacts/test-results `
  --logger trx
```
