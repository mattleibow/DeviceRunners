using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

/// <summary>
/// Message sink that forwards xUnit v3 diagnostic and internal diagnostic messages
/// to a DeviceRunners <see cref="IDiagnosticsManager"/>.
/// </summary>
class Xunit3DiagnosticMessageSink : IMessageSink
{
readonly Action<string> _logger;
readonly string _assemblyDisplayName;

public Xunit3DiagnosticMessageSink(Action<string> logger, string assemblyDisplayName)
{
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_assemblyDisplayName = assemblyDisplayName;
}

public bool OnMessage(IMessageSinkMessage message)
{
if (message is IDiagnosticMessage diagnosticMessage)
{
_logger($"{_assemblyDisplayName}: {diagnosticMessage.Message}");
}
else if (message is IInternalDiagnosticMessage internalDiagnosticMessage)
{
_logger($"{_assemblyDisplayName} [internal]: {internalDiagnosticMessage.Message}");
}

return true;
}
}
