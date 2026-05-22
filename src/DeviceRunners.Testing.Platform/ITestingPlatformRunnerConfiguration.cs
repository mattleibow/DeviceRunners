using Microsoft.Testing.Platform.Builder;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Read-only configuration produced by the builder.
/// Contains the test framework factory and MTP builder configurations.
/// </summary>
public interface ITestingPlatformRunnerConfiguration
{
	/// <summary>
	/// Factory that creates and runs the MTP TestApplication with the given args and builder configuration.
	/// Set by framework extension methods (AddXunit3, AddMSTest, AddNUnit).
	/// </summary>
	Func<string[], Action<ITestApplicationBuilder>, Task<int>>? TestFrameworkFactory { get; }

	/// <summary>
	/// Callbacks that configure the MTP builder (e.g., adding IDataConsumer for streaming).
	/// </summary>
	IReadOnlyList<Action<ITestApplicationBuilder>> BuilderConfigurations { get; }
}
