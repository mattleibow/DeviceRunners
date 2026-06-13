using System.Reflection;

namespace DeviceRunners.VisualRunners;

public interface IVisualTestRunnerConfiguration
{
	/// <summary>
	/// The list of assemblies that contain tests.
	/// </summary>
	IReadOnlyList<Assembly> TestAssemblies { get; }

	bool AutoStart { get; }

	bool AutoTerminate { get; }

	/// <summary>
	/// An optional <c>dotnet test --filter</c> style expression. When set, an auto-started
	/// run executes only the matching test cases instead of the entire suite.
	/// </summary>
	string? TestCaseFilter { get; }
}
