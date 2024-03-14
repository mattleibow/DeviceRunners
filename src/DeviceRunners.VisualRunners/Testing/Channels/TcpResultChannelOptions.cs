namespace DeviceRunners.VisualRunners;

public record TcpResultChannelOptions
{
	public string? HostName { get; init; }

	public IReadOnlyList<string>? HostNames { get; init; }

	public int Port { get; init; }

	public IResultChannelFormatter? Formatter { get; init; }

	public bool Required { get; init; }
}
