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

        // Install the main app (dependencies should be installed separately if needed)
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

    public void InstallDependencies(string msixPath, Action<string>? logger = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Dependency installation is only supported on Windows.");
        }

        // Determine architecture
        var arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
        if (arch == "AMD64")
        {
            arch = "x64";
        }

        // Look for dependencies folder relative to the MSIX file
        var msixDirectory = Path.GetDirectoryName(msixPath);
        if (msixDirectory == null) return;

        var dependenciesPath = Path.Combine(msixDirectory, "..", "Dependencies", arch ?? "x64");
        if (!Directory.Exists(dependenciesPath)) return;

        // Install each dependency
        var dependencyFiles = Directory.GetFiles(dependenciesPath, "*.msix");
        foreach (var dependencyFile in dependencyFiles)
        {
            try
            {
                logger?.Invoke($"    Installing dependency: '{dependencyFile}'");
                
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"Add-AppxPackage -Path '{dependencyFile}'\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (process != null)
                {
                    process.WaitForExit();
                    // Note: We don't throw on dependency install failure, just continue like PowerShell script
                    if (process.ExitCode != 0)
                    {
                        logger?.Invoke("    Dependency failed to install, continuing...");
                    }
                }
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