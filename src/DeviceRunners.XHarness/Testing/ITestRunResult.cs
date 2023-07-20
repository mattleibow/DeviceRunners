namespace DeviceRunners.XHarness;

public interface ITestRunResult
{
	string? this[string key] { get; set; }

	bool ContainsKey(string key);

	Dictionary<string, string> ToDictionary();
}
