namespace DeviceRunners.VisualRunners.Xunit;

class WasmXunitAssemblyInfo : ITestAssemblyInfo
{
	public WasmXunitAssemblyInfo(string assemblyFileName)
	{
		AssemblyFileName = assemblyFileName;
	}

	public string AssemblyFileName { get; }

	public ITestAssemblyConfiguration? Configuration => null;

	public IReadOnlyList<ITestCaseInfo> TestCases => [];
}
