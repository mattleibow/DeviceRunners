using System.Reflection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRunners.VisualRunners;

public class WebAssemblyVisualTestRunnerConfigurationBuilder : IVisualTestRunnerConfigurationBuilder
{
	readonly WebAssemblyHostBuilder _appBuilder;
	readonly List<Assembly> _testAssemblies = new();
	bool _autoStart;
	bool _autoTerminate;

	public WebAssemblyVisualTestRunnerConfigurationBuilder(WebAssemblyHostBuilder appBuilder)
	{
		_appBuilder = appBuilder;
	}

	internal VisualTestRunnerUsage RunnerUsage { get; set; } = VisualTestRunnerUsage.Automatic;

	void IVisualTestRunnerConfigurationBuilder.AddTestAssembly(Assembly assembly) =>
		_testAssemblies.Add(assembly);

	void IVisualTestRunnerConfigurationBuilder.AddTestPlatform<TTestDiscoverer, TTestRunner>()
	{
		_appBuilder.Services.AddSingleton<ITestDiscoverer, TTestDiscoverer>();
		_appBuilder.Services.AddSingleton<ITestRunner, TTestRunner>();
	}

	void IVisualTestRunnerConfigurationBuilder.EnableAutoStart(bool autoTerminate)
	{
		_autoStart = true;
		_autoTerminate = autoTerminate;
	}

	void IVisualTestRunnerConfigurationBuilder.AddResultChannel<T>(Func<IServiceProvider, T> creator) =>
		_appBuilder.Services.AddSingleton<IResultChannel>(svc => creator(svc));

	IVisualTestRunnerConfiguration IVisualTestRunnerConfigurationBuilder.Build() =>
		Build();

	public VisualTestRunnerConfiguration Build() =>
		new(_testAssemblies, _autoStart, _autoTerminate);
}