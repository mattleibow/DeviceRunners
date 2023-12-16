namespace DeviceRunners.VisualRunners;

public class DiagnosticsManager : IDiagnosticsManager
{
	public event EventHandler<string>? DiagnosticMessageReceived;

	public void PostDiagnosticMessage(string message)
	{
		DiagnosticMessageReceived?.Invoke(this, message);
	}
}
