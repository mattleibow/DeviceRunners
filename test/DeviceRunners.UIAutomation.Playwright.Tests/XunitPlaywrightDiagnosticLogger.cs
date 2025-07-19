using DeviceRunners.UIAutomation;

using Xunit.Abstractions;

namespace UIAutomationPlaywrightTests;

public class XunitPlaywrightDiagnosticLogger : IDiagnosticLogger
{
	private readonly ITestOutputHelper _output;

	public XunitPlaywrightDiagnosticLogger(ITestOutputHelper output)
	{
		_output = output;
	}

	public void Log(string message)
	{
		_output.WriteLine(message);
	}
}
