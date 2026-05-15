using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class DiagnosticMessageSink : DiagnosticEventSink, IMessageSink
{
	public DiagnosticMessageSink(IDiagnosticsManager? diagnosticsManager)
	{
		if (diagnosticsManager is not null)
		{
			DiagnosticMessageEvent += args => diagnosticsManager.PostDiagnosticMessage(args.Message.Message);
		}
	}

	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IDiagnosticMessage)
		{
			return ((IMessageSinkWithTypes)this).OnMessageWithTypes(message, null!);
		}
		return true;
	}
}
