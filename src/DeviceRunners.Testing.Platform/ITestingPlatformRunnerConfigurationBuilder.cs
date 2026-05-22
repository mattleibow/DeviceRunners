using Microsoft.Testing.Platform.Builder;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Builder interface for configuring the MTP device runner.
/// Framework packages extend this with AddXunit3(), AddMSTest(), etc.
/// Host packages (MAUI, Blazor) provide the concrete implementation.
/// </summary>
public interface ITestingPlatformRunnerConfigurationBuilder
{
	/// <summary>
	/// Registers a test framework runner factory. Called by framework extension methods.
	/// Only one framework may be registered per builder.
	/// </summary>
	void UseTestFramework(Func<string[], Action<ITestApplicationBuilder>, Task<int>> factory);

	/// <summary>
	/// Adds a callback that configures the MTP builder (e.g., adding IDataConsumer).
	/// </summary>
	void AddBuilderConfiguration(Action<ITestApplicationBuilder> configure);

	/// <summary>
	/// Builds the final configuration. Throws if no framework or multiple frameworks registered.
	/// </summary>
	ITestingPlatformRunnerConfiguration Build();
}
