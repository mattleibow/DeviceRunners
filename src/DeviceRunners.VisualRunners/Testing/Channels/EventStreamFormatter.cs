using System.Globalization;
using System.Text.Json;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// An <see cref="IResultChannelFormatter"/> that writes test events as newline-delimited JSON (NDJSON).
/// Each call writes exactly one JSON line and is immediately available to the underlying writer.
/// Designed for streaming over TCP where the CLI parses events as they arrive.
/// </summary>
public class EventStreamFormatter : IResultChannelFormatter
{
	TextWriter? _writer;

	public void BeginTestRun(TextWriter writer, string? message = null)
	{
		_writer = writer;

		var evt = new TestResultEvent
		{
			Type = TestResultEvent.TypeBegin,
			Message = message,
			Timestamp = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
		};

		_writer.WriteLine(JsonSerializer.Serialize(evt));
	}

	public void RecordResult(ITestResultInfo result)
	{
		if (_writer is null)
			return;

		var evt = new TestResultEvent
		{
			Type = TestResultEvent.TypeResult,
			DisplayName = result.TestCase.DisplayName,
			Assembly = result.TestCase.TestAssembly.AssemblyFileName,
			Status = result.Status switch
			{
				TestResultStatus.Passed => "Passed",
				TestResultStatus.Failed => "Failed",
				TestResultStatus.Skipped => "Skipped",
				_ => result.Status.ToString(),
			},
			Duration = result.Duration.ToString("c", CultureInfo.InvariantCulture),
			Output = result.Output,
			ErrorMessage = result.ErrorMessage,
			ErrorStackTrace = result.ErrorStackTrace,
			SkipReason = result.SkipReason,
			Timestamp = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
		};

		_writer.WriteLine(JsonSerializer.Serialize(evt));
	}

	public void EndTestRun()
	{
		if (_writer is null)
			return;

		var evt = new TestResultEvent
		{
			Type = TestResultEvent.TypeEnd,
			Timestamp = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
		};

		_writer.WriteLine(JsonSerializer.Serialize(evt));
	}
}
