using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace DeviceRunners.Cli.Services;

public class AppService
{
    private readonly PowerShellService _powerShellService;

    public AppService()
    {
        _powerShellService = new PowerShellService();
    }
    public string GetAppIdentityFromMsix(string msixPath)
    {
        using var archive = ZipFile.OpenRead(msixPath);
        var manifestEntry = archive.GetEntry("AppxManifest.xml");
        
        if (manifestEntry == null)
        {
            throw new InvalidOperationException("AppxManifest.xml not found in MSIX package.");
        }

        using var stream = manifestEntry.Open();
        var manifestXml = XDocument.Load(stream);
        
        var identity = manifestXml.Root?.Element(XName.Get("Identity", "http://schemas.microsoft.com/appx/manifest/foundation/windows10"));
        var appName = identity?.Attribute("Name")?.Value;
        
        if (string.IsNullOrEmpty(appName))
        {
            throw new InvalidOperationException("Unable to read app identity from MSIX manifest.");
        }

        return appName;
    }

    public string GetCertificateFromMsix(string msixPath)
    {
        var certPath = Path.ChangeExtension(msixPath, ".cer");
        if (!File.Exists(certPath))
        {
            throw new FileNotFoundException($"Certificate file not found: {certPath}");
        }
        return certPath;
    }

    public string GetCertificateFingerprint(string certPath)
    {
        var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
        return cert.Thumbprint;
    }

    public bool IsAppInstalled(string appIdentity)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            var output = _powerShellService.ExecuteCommand($"Get-AppxPackage -Name '{appIdentity}'");
            return !string.IsNullOrWhiteSpace(output) && !output.Contains("No packages were found");
        }
        catch
        {
            // Ignore errors and assume not installed
            return false;
        }
    }

    public void UninstallApp(string appIdentity)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("App uninstallation is only supported on Windows.");
        }

        _powerShellService.ExecuteCommand($"Get-AppxPackage -Name '{appIdentity}' | Remove-AppxPackage");
    }

    public void InstallCertificate(string certPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Certificate installation is only supported on Windows.");
        }

        // Try native C# approach first
        try
        {
            InstallCertificateNative(certPath);
        }
        catch
        {
            // Fall back to PowerShell with elevation
            _powerShellService.ExecuteCommand($"Import-Certificate -CertStoreLocation 'Cert:\\LocalMachine\\TrustedPeople' -FilePath '{certPath}'", requiresElevation: true);
        }
    }

    private void InstallCertificateNative(string certPath)
    {
        var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
        using var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadWrite);
        store.Add(cert);
        store.Close();
    }

    public void UninstallCertificate(string thumbprint)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Certificate uninstallation is only supported on Windows.");
        }

        // Try native C# approach first
        try
        {
            UninstallCertificateNative(thumbprint);
        }
        catch
        {
            // Fall back to PowerShell with elevation
            _powerShellService.ExecuteCommand($"Remove-Item -Path 'Cert:\\LocalMachine\\TrustedPeople\\{thumbprint}' -DeleteKey", requiresElevation: true);
        }
    }

    private void UninstallCertificateNative(string thumbprint)
    {
        using var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadWrite);
        
        var certsToRemove = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        foreach (var cert in certsToRemove)
        {
            store.Remove(cert);
        }
        
        store.Close();
    }

    public bool IsCertificateInstalled(string thumbprint)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            // Use native C# approach for checking certificate
            using var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            var found = certs.Count > 0;
            store.Close();
            return found;
        }
        catch
        {
            // Fall back to PowerShell
            try
            {
                var exitCode = _powerShellService.ExecuteCommandWithExitCode($"Test-Certificate 'Cert:\\LocalMachine\\TrustedPeople\\{thumbprint}'", out _, out _);
                return exitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }

    public void InstallApp(string msixPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("App installation is only supported on Windows.");
        }

        _powerShellService.ExecuteCommand($"Add-AppxPackage -Path '{msixPath}'");
    }

    public void InstallDependencies(string msixPath, Action<string>? logger = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Dependency installation is only supported on Windows.");
        }

        // Determine architecture using native C# instead of environment variable
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "x64" // Default fallback
        };

        // Look for dependencies folder relative to the MSIX file
        var msixDirectory = Path.GetDirectoryName(msixPath);
        if (msixDirectory == null) return;

        var dependenciesPath = Path.Combine(msixDirectory, "..", "Dependencies", arch);
        if (!Directory.Exists(dependenciesPath)) return;

        // Install each dependency
        var dependencyFiles = Directory.GetFiles(dependenciesPath, "*.msix");
        foreach (var dependencyFile in dependencyFiles)
        {
            try
            {
                logger?.Invoke($"    Installing dependency: '{dependencyFile}'");
                
                _powerShellService.ExecuteCommand($"Add-AppxPackage -Path '{dependencyFile}'");
            }
            catch
            {
                // Dependency failed to install, continuing like PowerShell script
                logger?.Invoke("    Dependency failed to install, continuing...");
            }
        }
    }

    public void StartApp(string appIdentity, string? launchArgs = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("App launching is only supported on Windows.");
        }

        var packageFamilyName = GetPackageFamilyName(appIdentity);
        var appUri = $"shell:AppsFolder\\{packageFamilyName}!App";

        var startInfo = new ProcessStartInfo
        {
            FileName = appUri,
            UseShellExecute = true
        };

        if (!string.IsNullOrEmpty(launchArgs))
        {
            startInfo.Arguments = launchArgs;
        }

        Process.Start(startInfo);
    }

    private string GetPackageFamilyName(string appIdentity)
    {
        var output = _powerShellService.ExecuteCommand($"(Get-AppxPackage -Name '{appIdentity}').PackageFamilyName");
        var familyName = output.Trim();
        
        if (string.IsNullOrEmpty(familyName))
        {
            throw new InvalidOperationException($"Failed to get package family name for app: {appIdentity}");
        }
        
        return familyName;
    }
}