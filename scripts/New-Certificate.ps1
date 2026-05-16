[CmdletBinding()]
param (
  [Parameter()]
  [String]$Publisher,
  [Parameter()]
  [String]$Manifest,
  [Parameter()]
  [String]$Project
)

$ErrorActionPreference = 'Stop'

Write-Host "============================================================"
Write-Host "PREPARATION"
Write-Host "============================================================"

# No certificate values were specified, so we are going to generate a self-signed certificate
if (-not $Publisher) {
  Write-Host "  - Determining publisher identity..."
  if (-not $Manifest) {
    if (-not $Project) {
      Write-Error "    No parameters were provided. Provide either the -Publisher or -Manifest values."
      exit 1
    }

    Write-Host "    No manifest was provided, trying to use the project '$Project'..."
    $possibleManifestPaths = @(
      # Windows project
      (Join-Path $Project '..\Package.appxmanifest'),
      # .NET MAUI single project
      (Join-Path $Project '..\Platforms\Windows\Package.appxmanifest')
    )
    foreach ($possible in $possibleManifestPaths) {
      if (Test-Path $possible) {
        $Manifest = Resolve-Path $possible
        Write-Host "    Trying the manifest path '$possible'..."
        Write-Host "    Manifest found: '$Manifest'"
        break
      } else {
        Write-Host "    Trying the manifest path '$possible'..."
      }
    }

    if (-not $Manifest) {
      Write-Error "    Unable to locate the Package.appxmanifest. Provide either the -Publisher or -Manifest values."
      exit 1
    }
  } elseif (-not (Test-Path $Manifest)) {
    Write-Error "    Invalid manifest provided: '$Manifest'."
    exit 1
  } else {
    Write-Host "    Reading publisher identity from the manifest: '$Manifest'..."
  }
  [xml]$manifestXml = (Get-Content $Manifest)
  $Publisher = $manifestXml.Package.Identity.Publisher
  Write-Host "    Publisher identity: '$Publisher'"
} else {
  Write-Host "  - Publisher identity provided: '$Publisher'"
}

Write-Host "  - Preparation complete."

Write-Host ""

Write-Host "============================================================"
Write-Host "GENERATE CERTIFICATE"
Write-Host "============================================================"

# Generate a certificate in the "My" store so we can use it to PUBLISH the app
Write-Host "  - Generating certificate..."
$cert = New-SelfSignedCertificate `
  -Type Custom `
  -Subject "$Publisher" `
  -KeyUsage DigitalSignature `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
$CertificateFingerprint = $cert.Thumbprint
Write-Host "    Publisher: '$Publisher'"
Write-Host "    Thumbprint: '$CertificateFingerprint'"
Write-Host "    Certificate generated."

Write-Host "  - Generation complete."

Write-Host ""

return $CertificateFingerprint
