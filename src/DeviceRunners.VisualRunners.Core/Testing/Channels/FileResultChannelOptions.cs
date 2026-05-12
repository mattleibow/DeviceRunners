namespace DeviceRunners.VisualRunners;

public record FileResultChannelOptions
{
	public required string FilePath { get; init; }

	public required IResultChannelFormatter Formatter { get; init; }
}
