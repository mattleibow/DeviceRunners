using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Source information provider that returns empty source info.
/// Source file/line mapping is unavailable when running via reflection
/// (no PDBs in WASM, no source link in embedded scenarios).
/// </summary>
class EmptySourceInformationProvider : global::Xunit.Sdk.LongLivedMarshalByRefObject, ISourceInformationProvider
{
	public static readonly EmptySourceInformationProvider Instance = new();

	public ISourceInformation GetSourceInformation(ITestCase testCase) => new global::Xunit.SourceInformation();

	public void Dispose() { }
}

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
