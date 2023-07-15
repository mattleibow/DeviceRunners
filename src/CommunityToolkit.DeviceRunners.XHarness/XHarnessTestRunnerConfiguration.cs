using System.Collections.ObjectModel;
using System.Reflection;

namespace CommunityToolkit.DeviceRunners.XHarness;

public class XHarnessTestRunnerConfiguration : IXHarnessTestRunnerConfiguration
{
	public XHarnessTestRunnerConfiguration(IEnumerable<Assembly> testAssemblies, IEnumerable<string>? skipCategories = null)
	{
		TestAssemblies = testAssemblies?.ToList() ?? throw new ArgumentNullException(nameof(testAssemblies));
		SkipCategories = skipCategories?.ToList() ?? new List<string>();
	}

	public IReadOnlyList<Assembly> TestAssemblies { get; }

	public IReadOnlyCollection<string> SkipCategories { get; }
}
