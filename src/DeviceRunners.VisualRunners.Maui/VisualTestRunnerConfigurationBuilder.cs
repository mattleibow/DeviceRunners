using System.Reflection;

namespace DeviceRunners.VisualRunners;

public class VisualTestRunnerConfigurationBuilder : IVisualTestRunnerConfigurationBuilder
{
	readonly MauiAppBuilder _appHostBuilder;
	readonly List<Assembly> _testAssemblies = new();
	readonly List<Func<ResourceDictionary>> _resourceDictionaryFactories = new();
	bool _autoStart;
	bool _autoTerminate;

	public VisualTestRunnerConfigurationBuilder(MauiAppBuilder appHostBuilder)
	{
		_appHostBuilder = appHostBuilder;
	}

	internal VisualTestRunnerUsage RunnerUsage { get; set; } = VisualTestRunnerUsage.Automatic;

	internal IReadOnlyList<Func<ResourceDictionary>> ResourceDictionaryFactories => _resourceDictionaryFactories;

	/// <summary>
	/// Registers a resource dictionary to be merged into Application.Resources at startup.
	/// Order matters — register Colors before Styles so StaticResource resolves correctly.
	/// </summary>
	public VisualTestRunnerConfigurationBuilder AddResourceDictionary<T>() where T : ResourceDictionary, new()
	{
		_resourceDictionaryFactories.Add(() => new T());
		return this;
	}

	/// <summary>
	/// Registers a resource dictionary instance to be merged into Application.Resources at startup.
	/// </summary>
	public VisualTestRunnerConfigurationBuilder AddResourceDictionary(ResourceDictionary dictionary)
	{
		ArgumentNullException.ThrowIfNull(dictionary);
		_resourceDictionaryFactories.Add(() => dictionary);
		return this;
	}

	/// <summary>
	/// Registers a resource dictionary using a factory to be merged into Application.Resources at startup.
	/// </summary>
	public VisualTestRunnerConfigurationBuilder AddResourceDictionary(Func<ResourceDictionary> factory)
	{
		_resourceDictionaryFactories.Add(factory ?? throw new ArgumentNullException(nameof(factory)));
		return this;
	}

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
