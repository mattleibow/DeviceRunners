[CmdletBinding()]
param (
  [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
  [String]$CertificateFingerprint
)

$ErrorActionPreference = 'Stop'

Write-Host "============================================================"
Write-Host "REMOVE CERTIFICATE"
Write-Host "============================================================"

try {
  Write-Host "  - Testing available certificates..."
  $certAvailable = Test-Certificate "Cert:\CurrentUser\My\$CertificateFingerprint" -AllowUntrustedRoot
  if ($certAvailable) {
    Write-Host "    Certificate was found."
    Write-Host "  - Removing certificate with fingerprint '$CertificateFingerprint'..."
    Remove-Item -Path "Cert:\CurrentUser\My\$CertificateFingerprint" -DeleteKey
    Write-Host "    Certificate removed."
  }
} catch {
  Write-Host "    Certificate was not found."
}

Write-Host ""
