namespace Xunit.Runner.Devices;

public interface IDiagnosticsManager
{
	void PostDiagnosticMessage(string message);

	event EventHandler<string>? DiagnosticMessageRecieved;
}
