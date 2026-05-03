using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// AOT-friendly JSON serialization context for <see cref="TestResultEvent"/>.
/// </summary>
[JsonSerializable(typeof(TestResultEvent))]
public partial class TestResultEventJsonContext : JsonSerializerContext;

/// <summary>
/// Represents a structured test event for the NDJSON event-streaming protocol.
/// Each event is serialized as a single JSON line over TCP.
/// </summary>
public record TestResultEvent
{
	[JsonPropertyName("type")]
	public string Type { get; init; } = "";

	[JsonPropertyName("message")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; init; }

	[JsonPropertyName("displayName")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? DisplayName { get; init; }

	[JsonPropertyName("assembly")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Assembly { get; init; }

	[JsonPropertyName("status")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Status { get; init; }

	[JsonPropertyName("duration")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Duration { get; init; }

	[JsonPropertyName("output")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Output { get; init; }

	[JsonPropertyName("errorMessage")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ErrorMessage { get; init; }

	[JsonPropertyName("errorStackTrace")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ErrorStackTrace { get; init; }

	[JsonPropertyName("skipReason")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? SkipReason { get; init; }

	[JsonPropertyName("timestamp")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Timestamp { get; init; }

	public const string TypeBegin = "begin";
	public const string TypeResult = "result";
	public const string TypeEnd = "end";

	// ── Factory methods ──────────────────────────────────────

	/// <summary>Creates a "begin" event.</summary>
	public static TestResultEvent Begin(string? message = null) =>
		new()
		{
			Type = TypeBegin,
			Message = message,
			Timestamp = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
		};

	/// <summary>Creates a "result" event from an <see cref="ITestResultInfo"/>.</summary>
	public static TestResultEvent FromInfo(ITestResultInfo result) =>
		new()
		{
			Type = TypeResult,
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

	/// <summary>Creates an "end" event.</summary>
	public static TestResultEvent End() =>
		new()
		{
			Type = TypeEnd,
			Timestamp = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
		};

	// ── Serialize / Parse ────────────────────────────────────

	/// <summary>
	/// Serializes this event to a JSON string using the AOT-friendly context.
	/// </summary>
	public override string ToString() =>
		JsonSerializer.Serialize(this, TestResultEventJsonContext.Default.TestResultEvent);

	/// <summary>
	/// Parses a single NDJSON line into a <see cref="TestResultEvent"/>.
	/// Returns null if the line is empty or not valid JSON.
	/// </summary>
	public static TestResultEvent? Parse(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
			return null;

		try
		{
			return JsonSerializer.Deserialize(line, TestResultEventJsonContext.Default.TestResultEvent);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Converts this "result" event back into an <see cref="ITestResultInfo"/>.
	/// </summary>
	public ITestResultInfo ToInfo()
	{
		var status = Status switch
		{
			"Passed" => TestResultStatus.Passed,
			"Failed" => TestResultStatus.Failed,
			"Skipped" => TestResultStatus.Skipped,
			_ => TestResultStatus.NotRun,
		};

		var duration = TimeSpan.Zero;
		if (Duration is not null)
			TimeSpan.TryParseExact(Duration, "c", CultureInfo.InvariantCulture, out duration);

		return new ParsedTestResultInfo(
			displayName: DisplayName ?? "",
			assemblyFileName: Assembly ?? "",
			status: status,
			duration: duration,
			output: Output,
			errorMessage: ErrorMessage,
			errorStackTrace: ErrorStackTrace,
			skipReason: SkipReason);
	}

	// ── Private reconstruction types ─────────────────────────

	sealed class ParsedTestResultInfo(
		string displayName,
		string assemblyFileName,
		TestResultStatus status,
		TimeSpan duration,
		string? output,
		string? errorMessage,
		string? errorStackTrace,
		string? skipReason) : ITestResultInfo
	{
		public ITestCaseInfo TestCase { get; } = new ParsedTestCaseInfo(displayName, assemblyFileName);
		public TestResultStatus Status { get; } = status;
		public TimeSpan Duration { get; } = duration;
		public string? Output { get; } = output;
		public string? ErrorMessage { get; } = errorMessage;
		public string? ErrorStackTrace { get; } = errorStackTrace;
		public string? SkipReason { get; } = skipReason;
	}

	sealed class ParsedTestCaseInfo(string displayName, string assemblyFileName) : ITestCaseInfo
	{
		public ITestAssemblyInfo TestAssembly { get; } = new ParsedTestAssemblyInfo(assemblyFileName);
		public string DisplayName { get; } = displayName;
		public ITestResultInfo? Result => null;
		public event Action<ITestResultInfo>? ResultReported { add { } remove { } }
	}

	sealed class ParsedTestAssemblyInfo(string assemblyFileName) : ITestAssemblyInfo
	{
		public string AssemblyFileName { get; } = assemblyFileName;
		public ITestAssemblyConfiguration? Configuration => null;
		public IReadOnlyList<ITestCaseInfo> TestCases => [];
	}
}
