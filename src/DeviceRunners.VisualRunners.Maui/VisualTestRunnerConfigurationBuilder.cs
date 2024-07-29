using System.Reflection;

namespace DeviceRunners.VisualRunners;

public class VisualTestRunnerConfigurationBuilder : IVisualTestRunnerConfigurationBuilder
{
	readonly MauiAppBuilder _appHostBuilder;
	readonly List<Assembly> _testAssemblies = new();
	bool _autoStart;
	bool _autoTerminate;

	public VisualTestRunnerConfigurationBuilder(MauiAppBuilder appHostBuilder)
	{
		_appHostBuilder = appHostBuilder;
	}

	internal VisualTestRunnerUsage RunnerUsage { get; set; } = VisualTestRunnerUsage.Automatic;

	void IVisualTestRunnerConfigurationBuilder.AddTestAssembly(Assembly assembly) =>
		_testAssemblies.Add(assembly);

	void IVisualTestRunnerConfigurationBuilder.AddTestPlatform<TTestDiscoverer, TTestRunner>()
	{
		_appHostBuilder.Services.AddSingleton<ITestDiscoverer, TTestDiscoverer>();
		_appHostBuilder.Services.AddSingleton<ITestRunner, TTestRunner>();
	}

	void IVisualTestRunnerConfigurationBuilder.EnableAutoStart(bool autoTerminate)
	{
		_autoStart = true;
		_autoTerminate = autoTerminate;
	}

	void IVisualTestRunnerConfigurationBuilder.AddResultChannel<T>(Func<IServiceProvider, T> creator) =>
		_appHostBuilder.Services.AddSingleton<IResultChannel>(svc => creator(svc));

	IVisualTestRunnerConfiguration IVisualTestRunnerConfigurationBuilder.Build() =>
		Build();

	public VisualTestRunnerConfiguration Build() =>
		new(_testAssemblies, _autoStart, _autoTerminate);
}
