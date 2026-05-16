using System.Reflection;

using Xunit.Sdk;
using Xunit.v3;

namespace DeviceRunners.VisualRunners.Xunit3;

/// <summary>
/// A test framework that creates WASM-safe <see cref="IXunitTestAssembly"/> instances
/// when <see cref="Assembly.Location"/> is empty (as is the case on WASM/Blazor).
/// Falls back to standard <see cref="XunitTestAssembly"/> on platforms where
/// <c>Assembly.Location</c> is available.
/// </summary>
class WasmXunit3TestFramework : XunitTestFramework
{
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

	static IXunitTestAssembly CreateTestAssembly(Assembly assembly)
	{
		var version = assembly.GetName().Version;

		if (!string.IsNullOrEmpty(assembly.Location))
			return new XunitTestAssembly(assembly, null, version);

		var logicalPath = assembly.GetName().Name + ".dll";
		return new WasmXunit3TestAssembly(assembly, null, version, logicalPath);
	}
}
