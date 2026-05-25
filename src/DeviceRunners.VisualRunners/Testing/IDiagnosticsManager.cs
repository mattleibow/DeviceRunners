namespace DeviceRunners.VisualRunners;

public interface IDiagnosticsManager
{
	void PostDiagnosticMessage(string message);

	/// <summary>
	/// Raised when a diagnostic message is posted. Handlers may be invoked on
	/// any thread (including xunit worker threads) and must not block.
	/// Use <see cref="SynchronizationContext.Post"/> to marshal to the UI thread.
	/// </summary>
	event EventHandler<string>? DiagnosticMessageReceived;
}
