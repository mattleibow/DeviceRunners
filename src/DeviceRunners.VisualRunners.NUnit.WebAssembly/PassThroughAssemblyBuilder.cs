using System.Reflection;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DeviceRunners.VisualRunners.NUnit;

class PassThroughAssemblyBuilder : ITestAssemblyBuilder
{
	readonly TestAssembly _testAssembly;

	public PassThroughAssemblyBuilder(TestAssembly testAssembly)
	{
		_testAssembly = testAssembly;
	}

	public ITest Build(Assembly assembly, IDictionary<string, object> options) => _testAssembly;

	public ITest Build(string assemblyName, IDictionary<string, object> options) => _testAssembly;
}
