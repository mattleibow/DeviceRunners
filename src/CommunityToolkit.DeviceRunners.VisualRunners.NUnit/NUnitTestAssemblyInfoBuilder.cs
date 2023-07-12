using System.Reflection;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;

namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

class NUnitTestAssemblyInfoBuilder : ITestAssemblyBuilder
{
	readonly NUnitTestAssemblyInfo _assemblyInfo;

	public NUnitTestAssemblyInfoBuilder(NUnitTestAssemblyInfo assemblyInfo)
	{
		_assemblyInfo = assemblyInfo;
	}

	public ITest Build(Assembly assembly, IDictionary<string, object> options) =>
		_assemblyInfo.TestAssembly;

	public ITest Build(string assemblyName, IDictionary<string, object> options) =>
		_assemblyInfo.TestAssembly;
}
