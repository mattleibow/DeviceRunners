namespace DeviceRunners.Cli.Models;

public class CertificateCreateResult : CommandResult
{
    public string? Publisher { get; set; }
    public string? Thumbprint { get; set; }
}