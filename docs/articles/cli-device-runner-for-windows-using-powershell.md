
> XHarness does not _yet_ support launching Windows tests, but this is easy to work around with a small powershell script. These scripts are found in the [`./scripts/`](https://github.com/mattleibow/DeviceRunners/tree/main/scripts) folder.

1. Build the app package for testing:
   ```
   dotnet publish <path/to/app.csproj> -f net9.0-windows10.0.<version>.0 -c Release -p:AppxPackageSigningEnabled=true
   ```
2. Run the tests:  
   ```
   ./scripts/Start-Tests.ps1 -AppPackage <path/to/app.msix> -OutputDirectory <path/to/output>
   ```
3. View test results in the output path:  
   ```
   <path/to/output>/TestResults.xml
   ```

To build and test the app at the path `sample\test\DeviceTestingKitApp.DeviceTests\DeviceTestingKitApp.DeviceTests.csproj` and get the test output at the path `artifacts` on my Windows laptop:

```
$fingerprint = .\scripts\New-Certificate.ps1 -Project sample\test\DeviceTestingKitApp.DeviceTests\DeviceTestingKitApp.DeviceTests.csproj
dotnet publish sample\test\DeviceTestingKitApp.DeviceTests\DeviceTestingKitApp.DeviceTests.csproj `
  -f net9.0-windows10.0.19041.0 `
  -c Release `
  -p:AppxPackageSigningEnabled=true `
  -p:PackageCertificateThumbprint=$fingerprint `
  -p:PackageCertificateKeyFile=""
./scripts/Remove-Certificate.ps1 -CertificateFingerprint $fingerprint

./scripts/Start-Tests.ps1 `
  -App sample\test\DeviceTestingKitApp.DeviceTests\bin\Release\net9.0-windows10.0.19041.0\win10-x64\AppPackages\DeviceTestingKitApp.DeviceTests_1.0.0.1_Test\DeviceTestingKitApp.DeviceTests_1.0.0.1_x64.msix `
  -OutputDirectory artifacts

# test result file will be artifacts/TestResults.xml
```

> If the app certificate is not installed, then an admin prompt will popup asking for permissions to install the certificate. If the test run is already elevated, then it will silently install (and uninstall).
