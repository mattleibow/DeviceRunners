using AndroidSdk;
using AndroidSdk.Apk;

namespace DeviceRunners.Cli.Services;

public class AndroidService
{
    private readonly Adb _adb;

    public AndroidService()
    {
        var androidSdkManager = new AndroidSdkManager();
        _adb = androidSdkManager.Adb;
    }

    public string GetPackageName(string apkPath)
    {
        var reader = new ApkReader(apkPath);
        var androidManifest = reader.ReadManifest();
        var manifest = androidManifest.Manifest;
        return manifest.PackageId;
    }

    public void UninstallApk(string packageName, string? adbSerial = null)
    {
        _adb.Uninstall(packageName, new Adb.AdbUninstallOptions(), adbSerial);
    }

    public void InstallApk(string apkPath, string? adbSerial = null)
    {
        _adb.Install(new FileInfo(apkPath), new Adb.AdbInstallOptions(), adbSerial);
    }

    public bool IsPackageInstalled(string packageName, string? adbSerial = null)
    {
        var packages = _adb.Shell($"pm list packages {packageName}", adbSerial);
        var expectedName = $"package:{packageName}";
        return packages.Any(p => p.Equals(expectedName, StringComparison.OrdinalIgnoreCase));
    }

    public void LaunchApp(string packageName, string? activityName = null, string? adbSerial = null)
    {
        var activity = activityName ?? $".MainActivity";
        var result = _adb.Shell($"am start -W -n {packageName}/{activity}", adbSerial);
    }
}
