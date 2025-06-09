namespace DeviceRunners.Cli.Models;

public class AppInstallResult : CommandResult
{
    public string? AppIdentity { get; set; }
    public string? AppPath { get; set; }
    public string? CertificateThumbprint { get; set; }
    public bool CertificateAutoInstalled { get; set; }
}