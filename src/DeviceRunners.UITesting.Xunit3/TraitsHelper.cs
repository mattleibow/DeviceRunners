namespace DeviceRunners.UITesting.Xunit3;

static class TraitsHelper
{
	/// <summary>
	/// Converts the read-only traits dictionary from xUnit v3 to a mutable
	/// Dictionary&lt;string, HashSet&lt;string&gt;&gt; as required by test case constructors.
	/// Replicates the internal xUnit v3 ToReadWrite() extension.
	/// </summary>
	public static Dictionary<string, HashSet<string>> ToReadWrite(
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits)
	{
		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		foreach (var kvp in traits)
			result[kvp.Key] = new HashSet<string>(kvp.Value, StringComparer.OrdinalIgnoreCase);
		return result;
	}
}
