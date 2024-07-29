using Xunit;

namespace DeviceRunners.VisualRunners.Xunit;

class DiagnosticMessageSink : DiagnosticEventSink
{
	public DiagnosticMessageSink(Action<string> logger, string assemblyDisplayName, bool showDiagnostics)
	{
		if (showDiagnostics && logger != null)
		{
			DiagnosticMessageEvent += args => logger($"{assemblyDisplayName}: {args.Message.Message}");
		}
	}
}
