namespace DeviceRunners.VisualRunners;

public class CompositeTestDiscoverer : ITestDiscoverer
{
	readonly IReadOnlyList<ITestDiscoverer> _testDiscoverers;

	public CompositeTestDiscoverer(IEnumerable<ITestDiscoverer> testDiscoverers)
	{
		_testDiscoverers = testDiscoverers.ToList();
	}

	public async Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default)
	{
		var assemblies = new List<ITestAssemblyInfo>();

		foreach (var discoverer in _testDiscoverers)
		{
			var result = await discoverer.DiscoverAsync(cancellationToken);

			assemblies.AddRange(result);
		}

		return assemblies;
	}
}
