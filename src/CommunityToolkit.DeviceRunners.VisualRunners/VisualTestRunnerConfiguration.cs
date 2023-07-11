using System.Reflection;

namespace CommunityToolkit.DeviceRunners.VisualRunners;

public class VisualTestRunnerConfiguration : IVisualTestRunnerConfiguration
{
	public VisualTestRunnerConfiguration(IReadOnlyList<Assembly> testAssemblies)
	{
		TestAssemblies = testAssemblies?.ToList() ?? throw new ArgumentNullException(nameof(testAssemblies));
	}

	public IReadOnlyList<Assembly> TestAssemblies { get; }
}
