using DeviceRunners.Cli.Services;
using System.Runtime.InteropServices;

namespace DeviceRunners.Cli.Tests;

public class CertificateServiceTests
{
    [Fact]
    public void CreateSelfSignedCertificate_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        // Arrange
        var service = new CertificateService();
        var publisher = "CN=Test Publisher";
        
        // Act & Assert
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Throws<PlatformNotSupportedException>(() => service.CreateSelfSignedCertificate(publisher));
        }
    }

    [Fact]
    public void RemoveCertificate_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        // Arrange
        var service = new CertificateService();
        var thumbprint = "dummy";
        
        // Act & Assert
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Throws<PlatformNotSupportedException>(() => service.RemoveCertificate(thumbprint));
        }
    }

    [Fact]
    public void CertificateExists_OnNonWindows_ReturnsFalse()
    {
        // Arrange
        var service = new CertificateService();
        var thumbprint = "dummy";
        
        // Act
        var result = service.CertificateExists(thumbprint);
        
        // Assert
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.False(result);
        }
    }
}