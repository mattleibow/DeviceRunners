using System.Reflection;

namespace DeviceRunners.XHarness;

public interface IXHarnessTestRunnerConfiguration
{
	/// <summary>
	/// The list of assemblies that contain tests.
	/// </summary>
	IReadOnlyList<Assembly> TestAssemblies { get; }

	/// <summary>
	/// The list of categories to skip in the form:
	///   [category-name]=[skip-when-value]
	/// </summary>
	IReadOnlyCollection<string> SkipCategories { get; }

	/// <summary>
	/// The directory where the test results file should be copied.
	/// </summary>
	string? OutputDirectory { get; }
}
