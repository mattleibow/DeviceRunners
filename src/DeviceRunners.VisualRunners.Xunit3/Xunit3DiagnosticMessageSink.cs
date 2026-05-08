using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3DiagnosticMessageSink : IMessageSink
{
	readonly Action<string>? _logger;
	readonly string _assemblyDisplayName;

	public Xunit3DiagnosticMessageSink(Action<string>? logger, string assemblyDisplayName)
	{
		_logger = logger;
		_assemblyDisplayName = assemblyDisplayName;
	}

	public bool OnMessage(IMessageSinkMessage message)
	{
		if (_logger is not null && message is IDiagnosticMessage diagnosticMessage)
		{
			_logger($"{_assemblyDisplayName}: {diagnosticMessage.Message}");
		}

		return true;
	}
}
