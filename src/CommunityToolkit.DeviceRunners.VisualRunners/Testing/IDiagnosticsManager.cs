namespace CommunityToolkit.DeviceRunners.VisualRunners;

public interface IDiagnosticsManager
{
	void PostDiagnosticMessage(string message);

	event EventHandler<string>? DiagnosticMessageRecieved;
}
