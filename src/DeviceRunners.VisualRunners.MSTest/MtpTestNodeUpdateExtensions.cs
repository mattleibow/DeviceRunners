using Microsoft.Testing.Platform.ServerMode.Client;

namespace DeviceRunners.VisualRunners.MSTest;

/// <summary>
/// Reads the parts of a server-mode test node that <see cref="MtpTestNodeUpdate"/> leaves on the raw
/// property bag — the source location and the traits — and classifies the node's execution state.
/// </summary>
static class MtpTestNodeUpdateExtensions
{
	/// <summary>True for a runnable test ("action"), false for a container ("group").</summary>
	public static bool IsAction(this MtpTestNodeUpdate node) =>
		string.Equals(node.NodeType, "action", StringComparison.Ordinal);

	public static bool IsDiscovered(this MtpTestNodeUpdate node) =>
		string.Equals(node.ExecutionState, "discovered", StringComparison.Ordinal);

	/// <summary>True for terminal execution states that carry a final result.</summary>
	public static bool IsTerminalResult(this MtpTestNodeUpdate node) => node.ExecutionState switch
	{
		"passed" or "skipped" or "failed" or "error" or "timed-out" or "canceled" => true,
		_ => false,
	};

	/// <summary>
	/// True when a later retry attempt for the same test node supersedes this update, so it is not the
	/// test's final outcome. MSTest's <c>[Retry]</c> reports every attempt under the same UID, tagging
	/// the earlier ones with the wire property <c>retry.is-superseded</c>. MTP's retry contract says
	/// single-result consumers (TRX, JUnit, the process exit code — and this runner) skip these, so a
	/// fail-then-pass retry reports only the final pass.
	/// </summary>
	public static bool IsSupersededRetry(this MtpTestNodeUpdate node) =>
		node.Node.TryGetValue("retry.is-superseded", out var value) && value is true;

	/// <summary>
	/// Joins the node's captured standard output and standard error (MSTest's MTP adapter reports
	/// <c>TestContext.WriteLine</c> and console writes through these), or returns <c>null</c> when the
	/// node carried neither.
	/// </summary>
	public static string? CombinedOutput(this MtpTestNodeUpdate node)
	{
		var parts = new[] { node.StandardOutput, node.StandardError }
			.Where(part => !string.IsNullOrEmpty(part));

		var combined = string.Join(Environment.NewLine, parts);
		return combined.Length == 0 ? null : combined;
	}

	/// <summary>Fully qualified declaring type name (namespace + type).</summary>
	public static string? GetLocationType(this MtpTestNodeUpdate node) =>
		node.GetString("location.type");

	/// <summary>Method name, possibly with a parameter-type list in parentheses.</summary>
	public static string? GetLocationMethod(this MtpTestNodeUpdate node) =>
		node.GetString("location.method");

	/// <summary>
	/// Reads the node's traits. Each wire element is an object whose property name is the trait key
	/// and whose value is the trait value. MSTest serializes [TestProperty("k","v")] as {"k":"v"},
	/// but [TestCategory("x")] as {"x":""} (name as key, empty value). To stay consistent with the
	/// NUnit/xunit backends — and so the runner's "Category" filter works — empty-valued traits are
	/// exposed as values of the conventional "Category" trait instead of as bare keys.
	/// </summary>
	public static IReadOnlyDictionary<string, IReadOnlyList<string>> GetTraits(this MtpTestNodeUpdate node)
	{
		if (!node.Node.TryGetValue("traits", out var traits) || traits is not IEnumerable<object?> traitElements)
			return new Dictionary<string, IReadOnlyList<string>>();

		var result = new Dictionary<string, List<string>>();
		foreach (var trait in traitElements.OfType<IDictionary<string, object?>>())
		{
			foreach (var property in trait)
			{
				var rawValue = property.Value as string;

				var (key, value) = string.IsNullOrEmpty(rawValue)
					? ("Category", property.Key)
					: (property.Key, rawValue!);

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

	static string? GetString(this MtpTestNodeUpdate node, string propertyName) =>
		node.Node.TryGetValue(propertyName, out var value) ? value as string : null;
}
