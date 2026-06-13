using System.Reflection;

namespace DeviceRunners.VisualRunners;

public interface IVisualTestRunnerConfigurationBuilder
{
	void AddTestAssembly(Assembly assembly);

	void AddTestPlatform<TTestDiscoverer, TTestRunner>()
		where TTestDiscoverer : class, ITestDiscoverer
		where TTestRunner : class, ITestRunner;

	void EnableAutoStart(bool autoTerminate = false);

	/// <summary>
	/// Sets a <c>dotnet test --filter</c> style expression used to select which tests run
	/// during an auto-started run.
	/// </summary>
	void SetTestCaseFilter(string? filter);

	void AddResultChannel<T>(Func<IServiceProvider, T> creator)
		where T : class, IResultChannel;

	IVisualTestRunnerConfiguration Build();
}
