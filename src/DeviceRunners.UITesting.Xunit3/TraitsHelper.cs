namespace DeviceRunners.UITesting.Xunit3;

static class TraitsHelper
{
	/// <summary>
	/// Converts the read-only traits dictionary from xUnit v3 to a mutable
	/// Dictionary&lt;string, HashSet&lt;string&gt;&gt; as required by test case constructors.
	/// Uses OrdinalIgnoreCase for keys (matching xUnit v3) but case-sensitive
	/// comparison for values (matching xUnit v3's default behavior).
	/// </summary>
	public static Dictionary<string, HashSet<string>> ToReadWrite(
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits)
	{
		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		foreach (var kvp in traits)
			result[kvp.Key] = new HashSet<string>(kvp.Value);
		return result;
	}
}
