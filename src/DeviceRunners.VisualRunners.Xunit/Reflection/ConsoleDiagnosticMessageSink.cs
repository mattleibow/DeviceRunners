using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Message sink that routes xunit diagnostic messages to an <see cref="IDiagnosticsManager"/>.
/// When no diagnostics manager is available, messages are silently discarded.
/// </summary>
class ConsoleDiagnosticMessageSink : global::Xunit.Sdk.LongLivedMarshalByRefObject, IMessageSink
{
	readonly IDiagnosticsManager? _diagnosticsManager;

	public static readonly ConsoleDiagnosticMessageSink Instance = new(null);

	public ConsoleDiagnosticMessageSink(IDiagnosticsManager? diagnosticsManager)
	{
		_diagnosticsManager = diagnosticsManager;
	}

	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IDiagnosticMessage diagnosticMessage)
		{
			_diagnosticsManager?.PostDiagnosticMessage(diagnosticMessage.Message);
		}
		return true;
	}
}
