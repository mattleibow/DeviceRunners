using System.Reflection;

namespace CommunityToolkit.DeviceRunners.XHarness;

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
}
