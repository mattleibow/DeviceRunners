namespace DeviceRunners.XHarness;

public class XHarnessTestRunResult : ITestRunResult
{
	readonly Dictionary<string, string> _data = new();

	public string? this[string key]
	{
		get => _data.TryGetValue(key, out var value) ? value : null;
		set
		{
			if (value is null)
				_data.Remove(key);
			else
				_data[key] = value;
		}
	}

	public bool ContainsKey(string key) =>
		_data.ContainsKey(key);

	public Dictionary<string, string> ToDictionary() =>
		new(_data);
}
