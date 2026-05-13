namespace DeviceRunners.VisualRunners;

/// <summary>
/// An <see cref="IResultChannel"/> that writes test results to <see cref="Console.Out"/>.
/// Defaults to human-readable text format; pass a different formatter for NDJSON or other formats.
/// </summary>
public class ConsoleResultChannel : TextWriterResultChannel
{
	public ConsoleResultChannel(IResultChannelFormatter? formatter = null)
		: base(formatter ?? new TextResultChannelFormatter())
	{
	}

	protected override TextWriter CreateWriter() => Console.Out;
}
