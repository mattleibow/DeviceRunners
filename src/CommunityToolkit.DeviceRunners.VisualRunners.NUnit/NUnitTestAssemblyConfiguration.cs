namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

class NUnitTestAssemblyConfiguration : ITestAssemblyConfiguration
{
	public NUnitTestAssemblyConfiguration(IDictionary<string, object> configuration)
	{
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
	}

	public IDictionary<string, object> Configuration { get; }
}
