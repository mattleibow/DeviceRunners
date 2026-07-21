using System.Text.Json;

namespace DeviceRunners.VisualRunners.MSTest;

/// <summary>
/// A test node as it crosses the Microsoft.Testing.Platform server-mode JSON-RPC channel
/// (the <c>node</c> object inside a <c>testing/testUpdates/tests</c> notification).
/// </summary>
readonly struct WireTestNode
{
	WireTestNode(
		string uid,
		string displayName,
		string? nodeType,
		string? executionState,
		string? locationType,
		string? locationMethod,
		double? durationMs,
		string? errorMessage,
		string? errorStackTrace,
		IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
	{
		Uid = uid;
		DisplayName = displayName;
		NodeType = nodeType;
		ExecutionState = executionState;
		LocationType = locationType;
		LocationMethod = locationMethod;
		DurationMs = durationMs;
		ErrorMessage = errorMessage;
		ErrorStackTrace = errorStackTrace;
		Traits = traits;
	}

	public string Uid { get; }
	public string DisplayName { get; }

	/// <summary>"action" for a runnable test, "group" for a container.</summary>
	public string? NodeType { get; }

	/// <summary>discovered / in-progress / passed / skipped / failed / error / timed-out / canceled.</summary>
	public string? ExecutionState { get; }

	/// <summary>Fully qualified declaring type name (namespace + type).</summary>
	public string? LocationType { get; }

	/// <summary>Method name, possibly with a parameter-type list in parentheses.</summary>
	public string? LocationMethod { get; }

	public double? DurationMs { get; }
	public string? ErrorMessage { get; }
	public string? ErrorStackTrace { get; }
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	public bool IsAction => string.Equals(NodeType, "action", StringComparison.Ordinal);

	public bool IsDiscovered => string.Equals(ExecutionState, "discovered", StringComparison.Ordinal);

	/// <summary>True for terminal execution states that carry a final result.</summary>
	public bool IsTerminalResult => ExecutionState switch
	{
		"passed" or "skipped" or "failed" or "error" or "timed-out" or "canceled" => true,
		_ => false,
	};

	public static WireTestNode FromJson(JsonElement node)
	{
		var uid = GetString(node, "uid") ?? string.Empty;
		var displayName = GetString(node, "display-name") ?? uid;

		return new WireTestNode(
			uid,
			displayName,
			GetString(node, "node-type"),
			GetString(node, "execution-state"),
			GetString(node, "location.type"),
			GetString(node, "location.method"),
			GetDouble(node, "time.duration-ms"),
			GetString(node, "error.message"),
			GetString(node, "error.stacktrace"),
			ParseTraits(node));
	}

	static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseTraits(JsonElement node)
	{
		if (!node.TryGetProperty("traits", out var traits) || traits.ValueKind != JsonValueKind.Array)
			return new Dictionary<string, IReadOnlyList<string>>();

		// Each element is an object whose property name is the trait key and whose value is the
		// trait value. MSTest serializes [TestProperty("k","v")] as {"k":"v"}, but [TestCategory("x")]
		// as {"x":""} (name as key, empty value). To stay consistent with the NUnit/xunit backends —
		// and so the runner's "Category" filter works — expose empty-valued traits as values of the
		// conventional "Category" trait instead of as bare keys.
		var result = new Dictionary<string, List<string>>();
		foreach (var trait in traits.EnumerateArray())
		{
			if (trait.ValueKind != JsonValueKind.Object)
				continue;

			foreach (var property in trait.EnumerateObject())
			{
				var rawValue = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;

				var (key, value) = string.IsNullOrEmpty(rawValue)
					? ("Category", property.Name)
					: (property.Name, rawValue!);

				if (!result.TryGetValue(key, out var values))
				{
					values = new List<string>();
					result[key] = values;
				}

				values.Add(value);
			}
		}

		return result.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<string>)kvp.Value);
	}

	static string? GetString(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
			? value.GetString()
			: null;

	static double? GetDouble(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
			? value.GetDouble()
			: null;
}
