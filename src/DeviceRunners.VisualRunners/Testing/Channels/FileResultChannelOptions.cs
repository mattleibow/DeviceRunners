namespace DeviceRunners.VisualRunners;

public record FileResultChannelOptions
{
	public string? FilePath { get; init; }

	public IResultChannelFormatter? Formatter { get; init; }
}
