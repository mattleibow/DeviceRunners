namespace DeviceRunners.VisualRunners;

public interface ITestDiscoverer
{
	Task<IReadOnlyList<ITestAssemblyInfo>> DiscoverAsync(CancellationToken cancellationToken = default);
}
