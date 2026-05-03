using System.Text.Json.Serialization;

namespace DeviceRunners.VisualRunners;

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
}
