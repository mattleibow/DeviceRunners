namespace DeviceRunners.XHarness;

public static class TestRunResultExtensions
{
	public static Bundle ToBundle(this ITestRunResult result)
	{
		var bundle = new Bundle();

		var dic = result.ToDictionary();
		foreach (var pair in dic)
			bundle.PutString(pair.Key, pair.Value);

		return bundle;
	}
}
