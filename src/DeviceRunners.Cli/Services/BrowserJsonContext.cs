using System.Text.Json.Serialization;

namespace DeviceRunners.Cli.Services;

// Chrome DevTools Protocol (CDP) message types for BrowserService

/// <summary>Chrome /json endpoint response — describes a debuggable page.</summary>
class ChromeDebugPage
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("webSocketDebuggerUrl")]
	public string? WebSocketDebuggerUrl { get; set; }
}

/// <summary>CDP command sent to the browser (no params).</summary>
class CdpCommand
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("method")]
	public string Method { get; set; } = "";
}

/// <summary>CDP command sent to the browser (with params).</summary>
class CdpCommandWithParams<T>
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("method")]
	public string Method { get; set; } = "";

	[JsonPropertyName("params")]
	public T? Params { get; set; }
}

/// <summary>CDP event received from the browser.</summary>
class CdpEvent
{
	[JsonPropertyName("method")]
	public string? Method { get; set; }

	[JsonPropertyName("params")]
	public CdpEventParams? Params { get; set; }
}

class CdpEventParams
{
	[JsonPropertyName("args")]
	public CdpConsoleArg[]? Args { get; set; }
}

class CdpConsoleArg
{
	[JsonPropertyName("value")]
	public object? Value { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("unserializableValue")]
	public string? UnserializableValue { get; set; }
}

/// <summary>CDP navigate command params.</summary>
class CdpNavigateParams
{
	[JsonPropertyName("url")]
	public string Url { get; set; } = "";
}

[JsonSerializable(typeof(ChromeDebugPage[]))]
[JsonSerializable(typeof(CdpCommand))]
[JsonSerializable(typeof(CdpCommandWithParams<CdpNavigateParams>))]
[JsonSerializable(typeof(CdpEvent))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class BrowserJsonContext : JsonSerializerContext;
