using Xunit;
using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class DiagnosticMessageSink : DiagnosticEventSink, IMessageSink
{
	public DiagnosticMessageSink(Action<string> logger, string assemblyDisplayName, bool showDiagnostics)
	{
		if (showDiagnostics && logger != null)
		{
			DiagnosticMessageEvent += args => logger($"{assemblyDisplayName}: {args.Message.Message}");
		}
	}

	public DiagnosticMessageSink(IDiagnosticsManager? diagnosticsManager)
	{
		if (diagnosticsManager is not null)
		{
			DiagnosticMessageEvent += args => diagnosticsManager.PostDiagnosticMessage(args.Message.Message);
		}
	}

	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IDiagnosticMessage diagnosticMessage)
		{
			// Trigger the DiagnosticMessageEvent via the base class
			return ((IMessageSinkWithTypes)this).OnMessageWithTypes(message, null!);
		}
		return true;
	}
}
