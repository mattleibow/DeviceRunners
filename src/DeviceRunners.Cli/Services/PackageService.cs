using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DeviceRunners.Cli.Services;

public class PackageService
{
    private readonly PowerShellService _powerShellService = new();

    public string GetPackageIdentity(string msixPath)
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

    public bool IsPackageInstalled(string appIdentity)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            GetPackageFamilyName(appIdentity);
            return true;
        }
        catch
        {
            // If GetPackageFamilyName fails, the package is not installed
            return false;
        }
    }

    public void UninstallPackage(string appIdentity)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("App uninstallation is only supported on Windows.");
        }

        _powerShellService.ExecuteCommand($"Get-AppxPackage -Name '{appIdentity}' | Remove-AppxPackage");
    }

    public void InstallPackage(string msixPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Package installation is only supported on Windows.");
        }

        _powerShellService.ExecuteCommand($"Add-AppxPackage -Path '{msixPath}'");
    }

    public List<string> GetDependencies(string msixPath)
    {
        var dependencies = new List<string>();
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return dependencies;
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
        if (msixDirectory == null) return dependencies;

        var dependenciesPath = Path.Combine(msixDirectory, "Dependencies", arch);
        if (!Directory.Exists(dependenciesPath)) return dependencies;

        // Return list of dependency files
        var dependencyFiles = Directory.GetFiles(dependenciesPath, "*.msix");
        dependencies.AddRange(dependencyFiles);
        
        return dependencies;
    }

    public void LaunchApp(string appIdentity, string? launchArgs = null)
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

    public string GetCertificateFromMsix(string msixPath)
    {
        var certPath = Path.ChangeExtension(msixPath, ".cer");
        if (!File.Exists(certPath))
        {
            throw new FileNotFoundException($"Certificate file not found: {certPath}");
        }
        return certPath;
    }
}