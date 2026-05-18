namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Configuration for the TCP streaming consumer that sends test events to the host.
/// </summary>
public record TcpStreamingConsumerOptions
{
	public IReadOnlyList<string> HostNames { get; init; } = ["localhost"];

	public int Port { get; init; } = 16384;

	public int Retries { get; init; } = 3;

	public TimeSpan RetryTimeout { get; init; } = TimeSpan.FromSeconds(5);

	public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
