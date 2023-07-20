using System.Reflection;

namespace DeviceRunners.XHarness;

public class XHarnessTestRunnerConfiguration : IXHarnessTestRunnerConfiguration
{
	public XHarnessTestRunnerConfiguration(
		IEnumerable<Assembly> testAssemblies,
		string? outputDirectory = null,
		IEnumerable<string>? skipCategories = null)
	{
		TestAssemblies = testAssemblies?.ToList() ?? throw new ArgumentNullException(nameof(testAssemblies));
		OutputDirectory = outputDirectory;
		SkipCategories = skipCategories?.ToList() ?? new List<string>();
	}

	public IReadOnlyList<Assembly> TestAssemblies { get; }

	public IReadOnlyCollection<string> SkipCategories { get; }

	public string? OutputDirectory { get; }
}
