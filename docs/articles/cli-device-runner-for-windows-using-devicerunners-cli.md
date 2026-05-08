# Windows Testing with DeviceRunners CLI

> [!NOTE]
> This documentation was partially generated using AI and may contain mistakes or be missing information. Please verify commands and procedures before use, and report any issues or improvements needed.

This guide covers testing Windows applications using the DeviceRunners CLI tool. The tool supports both packaged (.msix) and unpackaged (.exe) Windows applications.

## Running Tests

1. Build the app for testing:  
   ```
   dotnet publish <path/to/app.csproj> -f net9.0-windows10.0.19041.0 -c Release
   ```

2. Run the tests:  
   ```
   device-runners windows test --app <path/to/app.[msix|exe]> --results-directory <path/to/output>
   ```

3. View test results:  
   ```
   <path/to/output>/TestResults.xml
   ```

## Complete Examples

### Testing a Packaged Application

To build and test a packaged app at `sample/SampleMauiApp/SampleMauiApp.csproj`:

```pwsh
# Create and install certificate automatically
$certResult = device-runners windows cert install `
  --project sample/SampleMauiApp/SampleMauiApp.csproj `
  --output Json
$fingerprint = ($certResult | ConvertFrom-Json).Thumbprint

# Build the packaged app
dotnet publish sample/SampleMauiApp/SampleMauiApp.csproj `
  -f net9.0-windows10.0.19041.0 `
  -c Release `
  -p:AppxPackageSigningEnabled=true `
  -p:PackageCertificateThumbprint=$fingerprint `
  -p:PackageCertificateKeyFile=""

# Clean up certificate after build
device-runners windows cert uninstall --fingerprint $fingerprint

# Run tests (the CLI tool will handle certificate installation automatically)
$msix = "sample\SampleMauiApp\bin\Release\net9.0-windows10.0.19041.0\win10-x64\AppPackages\SampleMauiApp_1.0.0.1_Test\SampleMauiApp_1.0.0.1_x64.msix"
device-runners windows test --app $msix --results-directory artifacts/test-results

# Test result file will be: artifacts/test-results/TestResults.xml
```

### Testing an Unpackaged Application

```pwsh
# Build the unpackaged app  
dotnet publish sample/SampleMauiApp/SampleMauiApp.csproj `
  -f net9.0-windows10.0.19041.0 `
  -c Release `
  -p:WindowsPackageType=None

# Run tests
$exe = "sample\SampleMauiApp\bin\Release\net9.0-windows10.0.19041.0\win10-x64\SampleMauiApp.exe"
device-runners windows test --app $exe --results-directory artifacts/test-results

# Test result file will be: artifacts/test-results/TestResults.xml
```
