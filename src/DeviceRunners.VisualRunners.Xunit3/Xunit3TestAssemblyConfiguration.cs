using Xunit.Runner.Common;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3TestAssemblyConfiguration : ITestAssemblyConfiguration
{
	public Xunit3TestAssemblyConfiguration(TestAssemblyConfiguration configuration)
	{
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
	}

	public TestAssemblyConfiguration Configuration { get; }
}
