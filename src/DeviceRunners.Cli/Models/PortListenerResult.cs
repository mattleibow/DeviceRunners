namespace DeviceRunners.Cli.Models;

public class PortListenerResult : CommandResult
{
    public int Port { get; set; }
    public string? ReceivedData { get; set; }
    public string? ResultsFile { get; set; }
}