using System.Reflection;

namespace DeviceRunners.VisualRunners;

public interface IVisualTestRunnerConfiguration
{
	/// <summary>
	/// The list of assemblies that contain tests.
	/// </summary>
	IReadOnlyList<Assembly> TestAssemblies { get; }

	IResultChannel? ResultChannel { get; }

	bool AutoStart { get; }

	bool AutoTerminate { get; }
}
