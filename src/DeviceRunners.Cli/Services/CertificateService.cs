using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace DeviceRunners.Cli.Services;

public class CertificateService
{
    private readonly PowerShellService _powerShellService = new();

    public string CreateSelfSignedCertificate(string publisher)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Certificate creation is only supported on Windows.");
        }

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(publisher, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Add key usage extension for digital signature (matches PowerShell -KeyUsage DigitalSignature)
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

        // Add enhanced key usage for code signing (matches PowerShell TextExtension "2.5.29.37={text}1.3.6.1.5.5.7.3.3")
        var eku = new OidCollection();
        eku.Add(new Oid("1.3.6.1.5.5.7.3.3")); // Code signing
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(eku, true));

        // Add basic constraints extension (matches PowerShell TextExtension "2.5.29.19={text}")
        // The PowerShell script uses empty text, which means no CA capability
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));

        // Add subject key identifier extension (often required for code signing certificates)
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

        // For code signing, we need to ensure the certificate has the right private key properties
        // Export and re-import with the proper key storage flags
        var pfxData = certificate.Export(X509ContentType.Pfx);
        certificate.Dispose();
        
        certificate = X509CertificateLoader.LoadPkcs12(pfxData, null, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

        // Install the certificate to CurrentUser\My store
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        store.Add(certificate);
        store.Close();

        return certificate.Thumbprint;
    }

    public bool RemoveCertificate(string thumbprint)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Certificate removal is only supported on Windows.");
        }

        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);

        var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        if (certificates.Count == 0)
        {
            return false;
        }

        foreach (var cert in certificates)
        {
            store.Remove(cert);
        }

        store.Close();
        return true;
    }

    public bool CertificateExists(string thumbprint)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        store.Close();

        return certificates.Count > 0;
    }

    public string GetCertificateFingerprint(string certPath)
    {
        var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
        return cert.Thumbprint;
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
}