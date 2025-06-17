namespace DeviceRunners.Cli.Models;

public abstract class CommandResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}