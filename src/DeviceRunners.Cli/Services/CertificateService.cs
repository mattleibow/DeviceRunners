using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace DeviceRunners.Cli.Services;

public class CertificateService
{
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

        var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

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
}