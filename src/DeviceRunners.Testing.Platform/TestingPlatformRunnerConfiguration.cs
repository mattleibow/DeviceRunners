using Microsoft.Testing.Platform.Builder;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Concrete implementation of <see cref="ITestingPlatformRunnerConfiguration"/>.
/// </summary>
public class TestingPlatformRunnerConfiguration : ITestingPlatformRunnerConfiguration
{
	public TestingPlatformRunnerConfiguration(
		Func<string[], Action<ITestApplicationBuilder>, Task<int>>? testFrameworkFactory,
		IReadOnlyList<Action<ITestApplicationBuilder>> builderConfigurations)
	{
		TestFrameworkFactory = testFrameworkFactory;
		BuilderConfigurations = builderConfigurations;
	}

	public Func<string[], Action<ITestApplicationBuilder>, Task<int>>? TestFrameworkFactory { get; }

	public IReadOnlyList<Action<ITestApplicationBuilder>> BuilderConfigurations { get; }
}
