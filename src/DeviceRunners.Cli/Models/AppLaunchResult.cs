namespace DeviceRunners.Cli.Models;

public class AppLaunchResult : CommandResult
{
    public string? AppIdentity { get; set; }
    public string? Arguments { get; set; }
}