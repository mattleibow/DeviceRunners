[CmdletBinding()]
param (
  [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
  [String]$App,
  [Parameter()]
  [String]$Certificate,
  [Parameter()]
  [String]$OutputDirectory = 'artifacts',
  [Parameter()]
  [ValidateSet("XHarness", "NonInteractiveVisual", "None")]
  [String]$TestingMode
)

$ErrorActionPreference = 'Stop'

Write-Host "============================================================"
Write-Host "PREPARATION"
Write-Host "============================================================"

if (-not $Certificate) {
  Write-Host "  - Determining certificate for MSIX installer..."
  $Certificate = [IO.Path]::ChangeExtension($App, ".cer")
  if ($PSVersionTable.PSEdition -eq "Core") {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(Resolve-Path $Certificate)
  } else {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cert.Import((Resolve-Path $Certificate))
  }
  $certFingerprint = $cert.Thumbprint

  $Certificate = Resolve-Path $Certificate
  Write-Host "    File path: '$Certificate'"
  Write-Host "    Thumbprint: '$certFingerprint'"
  Write-Host "    Certificate identified."
}

Write-Host "  - Determining app identity..."
$App = Resolve-Path $App
Write-Host "    MSIX installer: '$App'"
try {
  Add-Type -Assembly "System.IO.Compression.FileSystem"
  $msixZipFile = [IO.Compression.ZipFile]::OpenRead($App)
  $manifestEntry = $msixZipFile.Entries | Where-Object { $_.Name -eq "AppxManifest.xml"}
  $stream = $manifestEntry.Open()
  $reader = New-Object IO.StreamReader($stream)
  [xml]$manifestXml = $reader.ReadToEnd()
  $appIdentity = $manifestXml.Package.Identity.Name
} finally {
  if ($reader) {
    $reader.Close()
  }
  if ($stream) {
    $stream.Close()
  }
  if ($msixZipFile) {
    $msixZipFile.Dispose()
  }
}
Write-Host "    App identity found: '$appIdentity'"

Write-Host "  - Testing to see if the app is installed..."
$appInstalls = Get-AppxPackage -Name $appIdentity
if ($appInstalls) {
  $packageFullName = $appInstalls.PackageFullName
  Write-Host "    App was installed '$packageFullName', uninstalling..."
  Remove-AppxPackage -Package $packageFullName
  Write-Host "    Uninstall complete..."
} else {
  Write-Host "    App was not installed."
}

$windowsPrincipal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
$adminRole = [Security.Principal.WindowsBuiltInRole] "Administrator"
$isAdminRole = $windowsPrincipal.IsInRole($adminRole)

$autoinstalledCertificate = $false

try {
  Write-Host "  - Testing available certificates..."
  $certAvailable = Test-Certificate "Cert:\LocalMachine\TrustedPeople\$certFingerprint"
  if ($certAvailable) {
    Write-Host "    Certificate was found."
  }
} catch {
  $autoinstalledCertificate = $true
  Write-Host "    Certificate was not found, importing certificate..."
  if ($isAdminRole) {
    Import-Certificate -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -FilePath $Certificate | Out-Null
  } else {
    Start-Process powershell -Wait -Verb RunAs -ArgumentList "Import-Certificate -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -FilePath $Certificate"
  }
  Write-Host "    Certificate imported."
}

# Install the app
Write-Host "  - Installing dependencies..."
$arch = $env:PROCESSOR_ARCHITECTURE
if ($arch -eq "AMD64") {
  $arch = "x64"
}
$deps = Get-ChildItem "$App\..\Dependencies\$arch\*.msix"
foreach ($dep in $deps) {
  try {
    Write-Host "    Installing dependency: '$dep'"
    Add-AppxPackage -Path $dep
  } catch {
    Write-Host "    Dependency faild to install, continuing..."
  }
}
Write-Host "  - Installing application..."
Add-AppxPackage -Path $App
$appInstalls = Get-AppxPackage -Name $appIdentity
$packageFullName = $appInstalls.PackageFullName
$packageFamilyName = $appInstalls.PackageFamilyName
Write-Host "    Application installed: '$packageFullName'"

Write-Host "  - Preparation complete."

Write-Host ""

Write-Host "============================================================"
Write-Host "RUN TESTS"
Write-Host "============================================================"

# Start the app
Write-Host "  - Starting the application..."
New-Item -ItemType Directory $OutputDirectory -Force | Out-Null
$OutputDirectory = Resolve-Path $OutputDirectory
Remove-Item $OutputDirectory -Recurse -Force
$launchArgs = ""
if ($TestingMode -eq "XHarness") {
  $launchArgs = "--xharness --output-directory=`"$OutputDirectory`""
}
Start-Process "shell:AppsFolder\$packageFamilyName!App" -Args $launchArgs
Write-Host "    Application started."

if ($TestingMode -eq "NonInteractiveVisual") {
} elseif ($TestingMode -eq "XHarness") {
  # Wait for the tests to finish
  Write-Host "  - Waiting for test results..."
  Write-Host "------------------------------------------------------------"
  $lastLine = 0
  while (!(Test-Path "$OutputDirectory\TestResults.xml")) {
    Start-Sleep 0.6
    if (Test-Path $OutputDirectory\test-output-*.log) {
      $log = Get-ChildItem $OutputDirectory\test-output-*.log
      $lines = [string[]](Get-Content $log | Select-Object -Skip $lastLine)
      foreach ($line in $lines) {
        Write-Host $line
      }
      $lastLine += $lines.Length
    }
  }
  Write-Host "------------------------------------------------------------"
  Write-Host "  - Checking test results for failures..."
  Write-Host "    Results file: '$OutputDirectory\TestResults.xml'"
  [xml]$resultsXml = Get-Content "$OutputDirectory\TestResults.xml"
  $failed = $resultsXml.assemblies.assembly |
    Where-Object { $_.failed -gt 0 -or $_.error -gt 0 }
  if ($failed) {
    Write-Host "    There were test failures."
    $result = 1
  } else {
    Write-Host "    There were no test failures."
  }
  Write-Host "  - Tests complete."
} else {
  Write-Host "  - Waiting for the app to finalize..."
  Start-Sleep 5
  Write-Host "    Application finalized."
} 

Write-Host ""

Write-Host "============================================================"
Write-Host "CLEANUP"
Write-Host "============================================================"

# Tests are complete, uninstall the app
Write-Host "  - Uninstalling application..."
Remove-AppxPackage -Package $packageFullName
Write-Host "    Application uninstalled."

if ($autoinstalledCertificate) {
  # Clean up all generated certificates
  Write-Host "  - Removing installed certificates..."
  if ($isAdminRole) {
    Remove-Item -Path "Cert:\LocalMachine\TrustedPeople\$certFingerprint" -DeleteKey
  } else {
    Start-Process powershell -Wait -Verb RunAs -ArgumentList "Remove-Item -Path 'Cert:\LocalMachine\TrustedPeople\$certFingerprint' -DeleteKey"
  }
  Write-Host "    Installed certificates removed."
}
Write-Host "  - Cleanup complete."
Write-Host ""

exit $result
