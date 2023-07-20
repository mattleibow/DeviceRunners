[CmdletBinding()]
param (
  [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
  [String]$AppPackage,
  [Parameter()]
  [String]$AppCertificate,
  [Parameter()]
  [String]$ArtifactsPath = 'artifacts'
)

$ErrorActionPreference = 'Stop'

Write-Host "============================================================"
Write-Host "PREPARATION"
Write-Host "============================================================"

if (-not $AppCertificate) {
  Write-Host "  - Determining certificate for MSIX installer..."
  $AppCertificate = [IO.Path]::ChangeExtension($AppPackage, ".cer")
  if ($PSVersionTable.PSEdition -eq "Core") {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(Resolve-Path $AppCertificate)
  } else {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cert.Import((Resolve-Path $AppCertificate))
  }
  $certFingerprint = $cert.Thumbprint

  Write-Host "    File path: '$AppCertificate'"
  Write-Host "    Thumbprint: '$certFingerprint'"
  Write-Host "    Certificate identified."
}

Write-Host "  - Determining app identity..."
Write-Host "    MSIX installer: '$AppPackage'"
try {
  Add-Type -Assembly "System.IO.Compression.FileSystem"
  $msixZipFile = [IO.Compression.ZipFile]::OpenRead($AppPackage)
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
    Import-Certificate -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -FilePath $AppCertificate | Out-Null
  } else {
    Start-Process powershell -Wait -Verb RunAs -ArgumentList "Import-Certificate -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -FilePath $AppCertificate"
  }
  Write-Host "    Certificate imported."
}

# Install the app
Write-Host "  - Installing application..."
Add-AppxPackage -Path $AppPackage
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
$output = Resolve-Path $ArtifactsPath
Remove-Item $output -Recurse -Force
Start-Process "shell:AppsFolder\$packageFamilyName!App" -Args "--xharness --output-directory=`"$output`""
Write-Host "    Application started."

# Wait for the tests to finish
Write-Host "  - Waiting for test results..."
Write-Host "------------------------------------------------------------"
$lastLine = 0
while (!(Test-Path "$output\TestResults.xml")) {
  Start-Sleep 0.6
  if (Test-Path $output\test-output-*.log) {
    $log = Get-ChildItem $output\test-output-*.log
    $lines = [string[]](Get-Content $log | Select-Object -Skip $lastLine)
    foreach ($line in $lines) {
      Write-Host $line
    }
    $lastLine += $lines.Length
  }
}
Write-Host "------------------------------------------------------------"
Write-Host "  - Tests complete."

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
