using System.Reflection;

namespace CommunityToolkit.DeviceRunners.VisualRunners;

public interface IVisualTestRunnerConfiguration
{
	/// <summary>
	/// The list of assemblies that contain tests.
	/// </summary>
	IReadOnlyList<Assembly> TestAssemblies { get; }
}
