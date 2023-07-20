using System.Reflection;

namespace DeviceRunners.VisualRunners;

public class VisualTestRunnerConfiguration : IVisualTestRunnerConfiguration
{
	public VisualTestRunnerConfiguration(IReadOnlyList<Assembly> testAssemblies)
	{
		TestAssemblies = testAssemblies?.ToList() ?? throw new ArgumentNullException(nameof(testAssemblies));
	}

	public IReadOnlyList<Assembly> TestAssemblies { get; }
}
