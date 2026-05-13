using Xunit;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitWasmTestAssemblyInfo : ITestAssemblyInfo
{
	public XunitWasmTestAssemblyInfo(string assemblyFileName, TestAssemblyConfiguration configuration)
	{
		AssemblyFileName = assemblyFileName;
		Configuration = configuration;
	}

	public string AssemblyFileName { get; }

	public TestAssemblyConfiguration Configuration { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => new XunitTestAssemblyConfiguration(Configuration);

	public List<XunitWasmTestCaseInfo> TestCases { get; } = [];

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
