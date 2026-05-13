using System.Reflection;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitWasmTestAssemblyInfo : ITestAssemblyInfo
{
	List<XunitWasmTestCaseInfo> _testCases = [];

	public XunitWasmTestAssemblyInfo(string assemblyFileName)
	{
		AssemblyFileName = assemblyFileName;
	}

	public string AssemblyFileName { get; }

	public ITestAssemblyConfiguration? Configuration => null;

	public IReadOnlyList<ITestCaseInfo> TestCases => _testCases;

	internal void SetTestCases(List<XunitWasmTestCaseInfo> testCases) =>
		_testCases = testCases;
}
