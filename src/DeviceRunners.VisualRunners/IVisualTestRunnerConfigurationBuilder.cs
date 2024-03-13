using System.Reflection;

namespace DeviceRunners.VisualRunners;

public interface IVisualTestRunnerConfigurationBuilder
{
	void AddTestAssembly(Assembly assembly);

	void AddTestPlatform<TTestDiscoverer, TTestRunner>()
		where TTestDiscoverer : class, ITestDiscoverer
		where TTestRunner : class, ITestRunner;

	void EnableAutoStart(bool autoTerminate = false);

	void AddResultChannel<T>(Func<IServiceProvider, T> creator)
		where T : class, IResultChannel;

	IVisualTestRunnerConfiguration Build();
}
