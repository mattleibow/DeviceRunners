namespace DeviceRunners.VisualRunners;

public class TextResultChannelFormatter : IResultChannelFormatter
{
	static readonly char[] NewLineChars = ['\r', '\n'];

	TextWriter? _writer;

	int _failed;
	int _passed;
	int _skipped;

	public void BeginTestRun(TextWriter writer, string? message = null)
	{
		var timestamp = DateTime.Now;

		_writer = writer;

		// log initial device runtime data
		if (string.IsNullOrEmpty(message))
			_writer.WriteLine("[Runner executing]");
		else
			_writer.WriteLine("[Runner executing:\t{0}]", message);
		_writer.WriteLine("[Device Date/Time:\t{0}]", timestamp);

		// reset counters
		_failed = _passed = _skipped = 0;
	}

	public void RecordResult(ITestResultInfo result)
	{
		if (_writer is null)
			return;

		// write the result
		if (result.Status == TestResultStatus.Passed)
		{
			_writer.Write("\t[PASS] ");
			_passed++;
		}
		else if (result.Status == TestResultStatus.Skipped)
		{
			_writer.Write("\t[SKIPPED] ");
			_skipped++;
		}
		else if (result.Status == TestResultStatus.Failed)
		{
			_writer.Write("\t[FAIL] ");
			_failed++;
		}
		else
		{
			_writer.Write("\t[INFO] ");
		}
		_writer.Write(result.TestCase.DisplayName);

		// write the reason/message
		var message = result.ErrorMessage;
		if (!string.IsNullOrEmpty(message))
		{
			_writer.Write(" : {0}", message.Replace("\r", "\\r").Replace("\n", "\\n"));
		}
		_writer.WriteLine();

		// write the stack trace
		var stacktrace = result.ErrorStackTrace;
		if (!string.IsNullOrEmpty(stacktrace))
		{
			var lines = stacktrace.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				_writer.WriteLine("\t\t{0}", line);
			}
		}
	}

	public void EndTestRun()
	{
		if (_writer is null)
			return;

		var total = _passed + _failed; // ignored are *not* run

		// log closing data
		if (_failed > 0)
			_writer.WriteLine("Tests failed.");

		_writer.WriteLine("Tests run: {0} Passed: {1} Failed: {2} Skipped: {3}", total, _passed, _failed, _skipped);

		// clean up
		_writer = null;
	}
}
