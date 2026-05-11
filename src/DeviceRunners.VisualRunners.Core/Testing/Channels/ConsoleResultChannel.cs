namespace DeviceRunners.VisualRunners;

/// <summary>
/// An <see cref="IResultChannel"/> implementation that streams test events to <see cref="Console.Out"/>.
/// Useful for headless/CI scenarios and browser WASM where filesystem access is unavailable.
/// </summary>
public class ConsoleResultChannel : TextWriterResultChannel
{
	public ConsoleResultChannel()
		: base(new EventStreamFormatter())
	{
	}

	public ConsoleResultChannel(IResultChannelFormatter formatter)
		: base(formatter)
	{
	}

	protected override TextWriter CreateWriter() => Console.Out;
}
