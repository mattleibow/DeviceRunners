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
}
