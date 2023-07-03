namespace Xunit.Runner.Devices.VisualRunner;

public class DiagnosticsManager : IDiagnosticsManager
{
	public event EventHandler<string>? DiagnosticMessageRecieved;

	public void PostDiagnosticMessage(string message)
	{
		DiagnosticMessageRecieved?.Invoke(this, message);
	}
}
