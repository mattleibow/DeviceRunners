namespace CommunityToolkit.DeviceRunners.Xunit.VisualRunner;

public interface IDiagnosticsManager
{
	void PostDiagnosticMessage(string message);

	event EventHandler<string>? DiagnosticMessageRecieved;
}
