using System.Reflection;

namespace CommunityToolkit.DeviceRunners.XHarness;

public interface IXHarnessTestRunnerConfigurationBuilder
{
	void AddTestAssembly(Assembly assembly);

	void SkipCategory(string category, string skipValue);

	void AddTestPlatform<TTestRunner>()
		where TTestRunner : class, ITestRunner;

	IXHarnessTestRunnerConfiguration Build();
}

