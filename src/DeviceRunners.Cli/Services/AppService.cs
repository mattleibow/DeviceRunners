using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace DeviceRunners.Cli.Services;

public class AppService
{
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
        var cert = new X509Certificate2(certPath);
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
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Get-AppxPackage -Name '{appIdentity}'\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return !string.IsNullOrWhiteSpace(output) && !output.Contains("No packages were found");
            }
        }
        catch
        {
            // Ignore errors and assume not installed
        }

        return false;
    }

    public void UninstallApp(string appIdentity)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("App uninstallation is only supported on Windows.");
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"Get-AppxPackage -Name '{appIdentity}' | Remove-AppxPackage\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });

        if (process != null)
        {
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to uninstall app: {error}");
            }
        }
    }

    public void InstallCertificate(string certPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Certificate installation is only supported on Windows.");
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"Import-Certificate -CertStoreLocation 'Cert:\\LocalMachine\\TrustedPeople' -FilePath '{certPath}'\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            Verb = "runas" // Request elevation
        });

        if (process != null)
        {
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to install certificate: {error}");
            }
        }
    }

    public bool IsCertificateInstalled(string thumbprint)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Test-Certificate 'Cert:\\LocalMachine\\TrustedPeople\\{thumbprint}'\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
        catch
        {
            // Ignore errors and assume not installed
        }

        return false;
    }

    public void InstallApp(string msixPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("App installation is only supported on Windows.");
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"Add-AppxPackage -Path '{msixPath}'\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });

        if (process != null)
        {
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to install app: {error}");
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
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"(Get-AppxPackage -Name '{appIdentity}').PackageFamilyName\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        });

        if (process != null)
        {
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                return output;
            }
        }

        throw new InvalidOperationException($"Failed to get package family name for app: {appIdentity}");
    }
}