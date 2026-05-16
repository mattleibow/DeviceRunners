using System.Reflection;

using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

/// <summary>
/// A test framework that creates <see cref="InMemoryXunit3TestAssembly"/> instances
/// for platforms where assemblies are loaded from streams or bundles rather than
/// from disk (Android, iOS, WASM) and <see cref="Assembly.Location"/> is empty.
/// </summary>
class InMemoryXunit3TestFramework : XunitTestFramework
{
	/// <summary>
	/// Creates the appropriate <see cref="ITestFramework"/> for the given assembly.
	/// Uses <see cref="InMemoryXunit3TestFramework"/> when <see cref="Assembly.Location"/>
	/// is empty (Android, iOS, WASM), otherwise delegates to the standard
	/// <see cref="ExtensibilityPointFactory"/>.
	/// </summary>
	public static ITestFramework CreateForAssembly(Assembly assembly)
	{
		if (string.IsNullOrEmpty(assembly.Location))
			return new InMemoryXunit3TestFramework();

		return ExtensibilityPointFactory.GetTestFramework(assembly);
	}

	protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
	{
		var testAssembly = CreateTestAssembly(assembly);
		return new XunitTestFrameworkDiscoverer(testAssembly);
	}

	protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
	{
		var testAssembly = CreateTestAssembly(assembly);
		return new XunitTestFrameworkExecutor(testAssembly);
	}

	static InMemoryXunit3TestAssembly CreateTestAssembly(Assembly assembly)
	{
		var version = assembly.GetName().Version;
		var logicalPath = assembly.GetName().Name + ".dll";
		return new InMemoryXunit3TestAssembly(assembly, null, version, logicalPath);
	}
}
