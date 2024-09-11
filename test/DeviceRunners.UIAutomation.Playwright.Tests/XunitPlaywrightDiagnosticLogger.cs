using DeviceRunners.UIAutomation.Playwright;

using Xunit.Abstractions;

namespace UIAutomationPlaywrightTests;

public class XunitPlaywrightDiagnosticLogger : IPlaywrightDiagnosticLogger
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
