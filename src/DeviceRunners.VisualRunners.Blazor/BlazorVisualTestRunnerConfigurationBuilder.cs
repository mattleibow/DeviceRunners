using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.VisualRunners.Blazor;

/// <summary>
/// Configuration builder for Blazor-hosted visual test runners.
/// Mirrors the MAUI <see cref="VisualTestRunnerConfigurationBuilder"/> but registers
/// services into a plain <see cref="IServiceCollection"/>.
/// </summary>
public class BlazorVisualTestRunnerConfigurationBuilder : IVisualTestRunnerConfigurationBuilder
{
	readonly IServiceCollection _services;
	readonly List<Assembly> _assemblies = [];
	bool _autoStart;
	bool _autoTerminate;

	public BlazorVisualTestRunnerConfigurationBuilder(IServiceCollection services)
	{
		_services = services;
	}

	void IVisualTestRunnerConfigurationBuilder.AddTestAssembly(Assembly assembly) =>
		_assemblies.Add(assembly);

	void IVisualTestRunnerConfigurationBuilder.AddTestPlatform<TTestDiscoverer, TTestRunner>()
	{
		_services.AddSingleton<ITestDiscoverer, TTestDiscoverer>();
		_services.AddSingleton<ITestRunner, TTestRunner>();
	}

	void IVisualTestRunnerConfigurationBuilder.EnableAutoStart(bool autoTerminate)
	{
		_autoStart = true;
		_autoTerminate = autoTerminate;
	}

	void IVisualTestRunnerConfigurationBuilder.AddResultChannel<T>(Func<IServiceProvider, T> creator) =>
		_services.AddSingleton<IResultChannel>(svc => creator(svc));

	IVisualTestRunnerConfiguration IVisualTestRunnerConfigurationBuilder.Build() => Build();

	public VisualTestRunnerConfiguration Build() =>
		new(_assemblies, _autoStart, _autoTerminate);
}
