using Microsoft.Extensions.DependencyInjection;
using Microsoft.Testing.Platform.Builder;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// MAUI-specific builder. Implements <see cref="ITestingPlatformRunnerConfigurationBuilder"/>
/// and exposes the MauiAppBuilder's service collection.
/// </summary>
public class TestingPlatformRunnerConfigurationBuilder : ITestingPlatformRunnerConfigurationBuilder
{
	readonly MauiAppBuilder _appHostBuilder;
	readonly List<Action<ITestApplicationBuilder>> _builderConfigs = [];
	Func<string[], Action<ITestApplicationBuilder>, Task<int>>? _factory;

	public TestingPlatformRunnerConfigurationBuilder(MauiAppBuilder appHostBuilder)
	{
		_appHostBuilder = appHostBuilder;
	}

	public IServiceCollection Services => _appHostBuilder.Services;

	public void UseTestFramework(Func<string[], Action<ITestApplicationBuilder>, Task<int>> factory)
	{
		if (_factory is not null)
			throw new InvalidOperationException("Only one test framework may be registered per builder. Remove the duplicate AddXunit3(), AddMSTest(), or AddNUnit() call.");
		_factory = factory;
	}

	public void AddBuilderConfiguration(Action<ITestApplicationBuilder> configure)
		=> _builderConfigs.Add(configure);

	public ITestingPlatformRunnerConfiguration Build()
	{
		if (_factory is null)
			throw new InvalidOperationException("No test framework registered. Call AddXunit3(), AddMSTest(), or AddNUnit().");
		return new TestingPlatformRunnerConfiguration(_factory, _builderConfigs.AsReadOnly());
	}
}
