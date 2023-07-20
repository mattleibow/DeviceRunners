using System.Reflection;

namespace DeviceRunners.XHarness;

public interface IXHarnessTestRunnerConfigurationBuilder
{
	void AddTestAssembly(Assembly assembly);

	void SkipCategory(string category, string skipValue);

	void AddTestPlatform<TTestRunner>()
		where TTestRunner : class, ITestRunner;

	void SetOutputDirectory(string directory);

	IXHarnessTestRunnerConfiguration Build();
}
