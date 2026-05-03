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
		_writer.WriteLine(JsonSerializer.Serialize(TestResultEvent.Begin(message)));
	}

	public void RecordResult(ITestResultInfo result)
	{
		_writer?.WriteLine(JsonSerializer.Serialize(TestResultEvent.FromInfo(result)));
	}

	public void EndTestRun()
	{
		_writer?.WriteLine(JsonSerializer.Serialize(TestResultEvent.End()));
	}
}
