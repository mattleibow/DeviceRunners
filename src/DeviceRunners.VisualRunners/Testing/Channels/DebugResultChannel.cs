using System.Diagnostics;
using System.Text;

namespace DeviceRunners.VisualRunners;

class DebugResultChannel : TextWriterResultChannel
{
	public DebugResultChannel()
		: base(new TextResultChannelFormatter())
	{
	}

	protected override TextWriter CreateWriter() => new DebugTextWriter();

	class DebugTextWriter : TextWriter
	{
		public override Encoding Encoding => Encoding.Default;

		public override void Write(char value) => Debug.Write(value);

		public override void Write(string? value) => Debug.Write(value);

		public override void WriteLine() => Debug.WriteLine(string.Empty);

		public override void WriteLine(string? value) => Debug.WriteLine(value);

		public override void WriteLine(string format, object?[] args) => Debug.WriteLine(format, args);
	}
}
