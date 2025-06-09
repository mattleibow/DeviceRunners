namespace DeviceRunners.Cli.Models;

public class TestStartResult : CommandResult
{
    public string? AppIdentity { get; set; }
    public string? AppPath { get; set; }
    public string? CertificateThumbprint { get; set; }
    public string? ResultsDirectory { get; set; }
    public int TestFailures { get; set; }
    public string? TestResults { get; set; }
}