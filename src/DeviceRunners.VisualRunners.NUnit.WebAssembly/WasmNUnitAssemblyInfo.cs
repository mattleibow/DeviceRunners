namespace DeviceRunners.VisualRunners.NUnit;

class WasmNUnitAssemblyInfo : ITestAssemblyInfo
{
	public WasmNUnitAssemblyInfo(string assemblyFileName)
	{
		AssemblyFileName = assemblyFileName;
	}

	public string AssemblyFileName { get; }

	public ITestAssemblyConfiguration? Configuration => null;

	public IReadOnlyList<ITestCaseInfo> TestCases => [];
}
