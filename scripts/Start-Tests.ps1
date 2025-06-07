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
Write-Host "DEBUG - SCRIPT PARAMETERS"
Write-Host "============================================================"
Write-Host "  App: '$App'"
Write-Host "  Certificate: '$Certificate'"
Write-Host "  OutputDirectory: '$OutputDirectory'"
Write-Host "  TestingMode: '$TestingMode'"
Write-Host ""

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
  $packageFamilyName = $appInstalls.PackageFamilyName
  Write-Host "    App was installed '$packageFullName' ($packageFamilyName), uninstalling..."
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
  Write-Host "    DEBUG: Certificate path: '$Certificate'"
  Write-Host "    DEBUG: Certificate fingerprint: '$certFingerprint'"
  Write-Host "    DEBUG: Is admin role: $isAdminRole"
  if ($isAdminRole) {
    Import-Certificate -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -FilePath $Certificate | Out-Null
  } else {
    if (-not $Certificate) {
      throw "Certificate path is null or empty, cannot import certificate"
    }
    $importArgs = @("Import-Certificate", "-CertStoreLocation", "Cert:\LocalMachine\TrustedPeople", "-FilePath", "`"$Certificate`"")
    Write-Host "    DEBUG: Import ArgumentList: $($importArgs -join ' ')"
    if ($importArgs.Count -eq 0) {
      throw "ArgumentList is empty for certificate import"
    }
    Start-Process powershell -Wait -Verb RunAs -ArgumentList $importArgs
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
Write-Host "    Application installed: '$packageFullName' ($packageFamilyName)"

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
if ($launchArgs) {
  Start-Process "shell:AppsFolder\$packageFamilyName!App" -Args $launchArgs
} else {
  Start-Process "shell:AppsFolder\$packageFamilyName!App"
}
Write-Host "    Application started."

if ($TestingMode -eq "NonInteractiveVisual") {
  # Start TCP listener to capture test results
  Write-Host "  - Starting TCP listener on port 16384..."
  # Ensure output directory exists for TCP results
  New-Item -ItemType Directory $OutputDirectory -Force | Out-Null
  $tcpResultsFile = "$OutputDirectory\tcp-test-results.txt"
  $listenerJob = Start-Job -ScriptBlock {
    param($OutputFile)
    & "$using:PSScriptRoot\New-PortListener.ps1" -Port 16384 -Output $OutputFile -NonInteractive
  } -ArgumentList $tcpResultsFile
  
  Write-Host "  - Waiting for test results via TCP..."
  Write-Host "------------------------------------------------------------"
  
  # Wait for the TCP listener to receive results (with timeout)
  $timeout = 300 # 5 minutes timeout
  $elapsed = 0
  while ((Get-Job -Id $listenerJob.Id).State -eq "Running" -and $elapsed -lt $timeout) {
    Start-Sleep 1
    $elapsed++
    if ($elapsed % 30 -eq 0) {
      Write-Host "  Still waiting for test results... ($elapsed/$timeout seconds)"
    }
  }
  
  # Stop the listener job if it's still running
  if ((Get-Job -Id $listenerJob.Id).State -eq "Running") {
    Write-Host "  Timeout reached, stopping TCP listener..."
    Stop-Job -Id $listenerJob.Id
  }
  
  # Get any output from the listener job
  $jobOutput = Receive-Job -Id $listenerJob.Id
  if ($jobOutput) {
    Write-Host $jobOutput
  }
  Remove-Job -Id $listenerJob.Id
  
  Write-Host "------------------------------------------------------------"
  
  # Check if we received test results
  if (Test-Path $tcpResultsFile) {
    Write-Host "  - Analyzing TCP test results..."
    $tcpResults = Get-Content $tcpResultsFile -Raw
    Write-Host "    Results received via TCP:"
    Write-Host $tcpResults
    
    # Simple check for test failure indicators in the TCP results
    # The TCP channel sends formatted test results, look for failure indicators
    if ($tcpResults -match "Failed:\s*[1-9]" -or $tcpResults -match "failed:\s*[1-9]" -or $tcpResults -match "Error" -or $tcpResults -match "FAIL") {
      Write-Host "    Test failures detected in TCP results."
      $result = 1
    } else {
      Write-Host "    No test failures detected in TCP results."
    }
  } else {
    Write-Host "    No TCP results file found - test may have failed to start or connect."
    $result = 1
  }
  Write-Host "  - TCP tests complete."
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
  Write-Host "    DEBUG: Certificate fingerprint for removal: '$certFingerprint'"
  Write-Host "    DEBUG: Is admin role: $isAdminRole"
  if ($isAdminRole) {
    Remove-Item -Path "Cert:\LocalMachine\TrustedPeople\$certFingerprint" -DeleteKey
  } else {
    if (-not $certFingerprint) {
      throw "Certificate fingerprint is null or empty, cannot remove certificate"
    }
    $removeArgs = @("Remove-Item", "-Path", "`"Cert:\LocalMachine\TrustedPeople\$certFingerprint`"", "-DeleteKey")
    Write-Host "    DEBUG: Remove ArgumentList: $($removeArgs -join ' ')"
    if ($removeArgs.Count -eq 0) {
      throw "ArgumentList is empty for certificate removal"
    }
    Start-Process powershell -Wait -Verb RunAs -ArgumentList $removeArgs
  }
  Write-Host "    Installed certificates removed."
}
Write-Host "  - Cleanup complete."
Write-Host ""

exit $result
