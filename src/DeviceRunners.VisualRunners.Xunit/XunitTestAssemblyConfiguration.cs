using Xunit;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitTestAssemblyConfiguration : ITestAssemblyConfiguration
{
	public XunitTestAssemblyConfiguration(TestAssemblyConfiguration configuration)
	{
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
	}

	public TestAssemblyConfiguration Configuration { get; }
}
