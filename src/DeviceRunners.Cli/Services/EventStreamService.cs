using System.Text;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.Cli.Services;

/// <summary>
/// Processes an NDJSON event stream, reassembling TCP chunks into complete lines,
/// parsing <see cref="TestResultEvent"/>s, and raising events for each.
/// Owns no I/O — the command layer subscribes to events and decides what to do
/// (console output, file output via result channel, etc.).
/// </summary>
public class EventStreamService
{
	readonly StringBuilder _lineBuffer = new();

	/// <summary>Whether a "begin" event has been received.</summary>
	public bool HasStarted { get; private set; }

	/// <summary>Whether an "end" event has been received (clean completion).</summary>
	public bool HasEnded { get; private set; }

	/// <summary>Number of test results received so far.</summary>
	public int TotalCount { get; private set; }

	/// <summary>Number of failed test results received so far.</summary>
	public int FailedCount { get; private set; }

	/// <summary>Number of passed test results received so far.</summary>
	public int PassedCount { get; private set; }

	/// <summary>Number of skipped test results received so far.</summary>
	public int SkippedCount { get; private set; }

	/// <summary>Raised when a "begin" event is received.</summary>
	public event EventHandler<TestRunBeginEventArgs>? TestRunStarted;

	/// <summary>Raised when a "result" event is received and forwarded to the channel.</summary>
	public event EventHandler<TestResultRecordedEventArgs>? TestResultRecorded;

	/// <summary>Raised when an "end" event is received.</summary>
	public event EventHandler? TestRunEnded;

	/// <summary>Raised when a line cannot be parsed as a valid event.</summary>
	public event EventHandler<UnparseableLineEventArgs>? UnparseableLine;

	/// <summary>
	/// Feeds a chunk of raw TCP data into the line buffer.
	/// Complete NDJSON lines are extracted, parsed, and forwarded to the result channel.
	/// </summary>
	public void ReceiveData(string data)
	{
		_lineBuffer.Append(data);

		var buffered = _lineBuffer.ToString();
		var lastNewline = buffered.LastIndexOf('\n');
		if (lastNewline < 0)
			return;

		var completeData = buffered[..lastNewline];
		_lineBuffer.Clear();
		_lineBuffer.Append(buffered[(lastNewline + 1)..]);

		foreach (var line in completeData.Split('\n'))
		{
			var trimmedLine = line.TrimEnd('\r');
			if (!string.IsNullOrWhiteSpace(trimmedLine))
				ProcessLine(trimmedLine);
		}
	}

	/// <summary>
	/// Flushes any remaining buffered data as a final line.
	/// Call this when the TCP connection closes.
	/// </summary>
	public void Flush()
	{
		if (_lineBuffer.Length > 0)
		{
			var remaining = _lineBuffer.ToString().TrimEnd('\r');
			_lineBuffer.Clear();
			if (!string.IsNullOrWhiteSpace(remaining))
				ProcessLine(remaining);
		}
	}

	void ProcessLine(string line)
	{
		// Skip ping/probe messages from TcpResultChannel host selection
		if (line == "ping")
			return;

		var evt = TestResultEvent.Parse(line);
		if (evt is null)
		{
			UnparseableLine?.Invoke(this, new UnparseableLineEventArgs { Line = line });
			return;
		}

		switch (evt.Type)
		{
			case TestResultEvent.TypeBegin:
				HasStarted = true;
				TestRunStarted?.Invoke(this, new TestRunBeginEventArgs { Message = evt.Message });
				break;

			case TestResultEvent.TypeResult:
				var resultInfo = evt.ToInfo();
				TotalCount++;

				if (resultInfo.Status == TestResultStatus.Failed)
					FailedCount++;
				else if (resultInfo.Status == TestResultStatus.Passed)
					PassedCount++;
				else if (resultInfo.Status == TestResultStatus.Skipped)
					SkippedCount++;

				TestResultRecorded?.Invoke(this, new TestResultRecordedEventArgs
				{
					Result = resultInfo,
					Event = evt,
				});
				break;

			case TestResultEvent.TypeEnd:
				HasEnded = true;
				TestRunEnded?.Invoke(this, EventArgs.Empty);
				break;
		}
	}
}

public class TestRunBeginEventArgs : EventArgs
{
	public string? Message { get; init; }
}

public class TestResultRecordedEventArgs : EventArgs
{
	public required ITestResultInfo Result { get; init; }

	public required TestResultEvent Event { get; init; }
}

public class UnparseableLineEventArgs : EventArgs
{
	public required string Line { get; init; }
}
