namespace DeviceRunners.UIAutomation;

public class CompositeDiagnosticLogger : IDiagnosticLogger
{
	private readonly List<IDiagnosticLogger> _loggers;

	public CompositeDiagnosticLogger(IEnumerable<IDiagnosticLogger> loggers) =>
		_loggers = loggers.ToList();

	public void Log(string message)
	{
		foreach (var logger in _loggers)
			logger.Log(message);
	}
}
