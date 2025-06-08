namespace DeviceRunners.Cli.Models;

public class CertificateRemoveResult : CommandResult
{
    public string? Fingerprint { get; set; }
    public bool WasFound { get; set; }
}