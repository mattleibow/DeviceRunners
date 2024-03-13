
namespace DeviceRunners.VisualRunners;

class ConsoleResultChannel : TextWriterResultChannel
{
	public ConsoleResultChannel()
		: base(new TextResultChannelFormatter())
	{
	}

	protected override TextWriter CreateWriter() => Console.Out;
}
