using System.Reflection;

namespace CommunityToolkit.DeviceRunners.VisualRunners;

public interface IVisualTestRunnerConfigurationBuilder
{
	void AddTestAssembly(Assembly assembly);

	void AddTestPlatform<TTestDiscoverer, TTestRunner>()
		where TTestDiscoverer : class, ITestDiscoverer
		where TTestRunner : class, ITestRunner;

	IVisualTestRunnerConfiguration Build();
}

